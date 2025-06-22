using Firma.Data.Data;
using Firma.Data.Data.Shop;
using Firma.PortalWWW.Interfaces;
using Firma.PortalWWW.Models.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Firma.PortalWWW.Services
{
    public class CartService : ICartService
    {
        private readonly FirmaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;
        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        private const string CartSessionKey = "ShoppingCart";

        public CartService(FirmaContext context, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task AddToCartAsync(int productId, int quantity = 1)
        {
            if (quantity <= 0) return;

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                _logger.LogWarning($"Attempted to add non-existent product (ID: {productId}) to cart.");
                return;
            }
            if (!product.IsInStock)
            {
                _logger.LogWarning($"Attempted to add out-of-stock product (ID: {productId}) to cart.");
                return;
            }

            var userId = GetCurrentUserId();

            if (userId.HasValue) // zalogowany użytkownik - zapisz do bazy
            {
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(ci => ci.IdUser == userId.Value && ci.IdProduct == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    _context.Cart.Add(new Cart
                    {
                        IdUser = userId.Value,
                        IdProduct = productId,
                        Quantity = quantity
                    });
                }
                await _context.SaveChangesAsync();
            }
            else // niezalogowany użytkownik - zapisz w sesji
            {
                var cart = GetSessionCart();
                var cartItem = cart.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cart.Add(new CartSessionItem
                    {
                        ProductId = productId,
                        ProductName = product.Name,
                        UnitPrice = product.Price,
                        Quantity = quantity,
                        ProductPhotoUrl = product.PhotoUrl
                    });
                }
                SaveSessionCart(cart);
            }
            _logger.LogInformation($"Product (ID: {productId}, Qty: {quantity}) added to cart for user/session.");
        }

        public async Task<List<CartItemViewModel>> GetCartItemsAsync()
        {
            var userId = GetCurrentUserId();
            var cartItemsViewModel = new List<CartItemViewModel>();

            if (userId.HasValue) // Logged-in user
            {
                var dbCartItems = await _context.Cart
                    .Where(ci => ci.IdUser == userId.Value)
                    .Include(ci => ci.Product)
                    .ToListAsync();

                foreach (var item in dbCartItems)
                {
                    if (item.Product != null)
                    {
                        cartItemsViewModel.Add(new CartItemViewModel
                        {
                            ProductId = item.IdProduct,
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.Product.Price, // Current product price
                            TotalPrice = item.Quantity * item.Product.Price,
                            ProductPhotoUrl = item.Product.PhotoUrl
                        });
                    }
                }
            }
            else // Anonymous user
            {
                var sessionCart = GetSessionCart();
                foreach (var item in sessionCart)
                {
                    cartItemsViewModel.Add(new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice, // Price at the time of adding
                        TotalPrice = item.Quantity * item.UnitPrice,
                        ProductPhotoUrl = item.ProductPhotoUrl
                    });
                }
            }
            return cartItemsViewModel;
        }

        public async Task RemoveFromCartAsync(int productId)
        {
            var userId = GetCurrentUserId();

            if (userId.HasValue) // Logged-in user
            {
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(ci => ci.IdUser == userId.Value && ci.IdProduct == productId);

                if (cartItem != null)
                {
                    _context.Cart.Remove(cartItem);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Removed product (ID: {productId}) from cart for user ID: {userId.Value}.");
                }
            }
            else // Anonymous user
            {
                var cart = GetSessionCart();
                var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);

                if (itemToRemove != null)
                {
                    cart.Remove(itemToRemove);
                    SaveSessionCart(cart);
                    _logger.LogInformation($"Removed product (ID: {productId}) from session cart.");
                }
            }
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                await RemoveFromCartAsync(productId);
                return;
            }

            var userId = GetCurrentUserId();

            if (userId.HasValue) // Logged-in user
            {
                var cartItem = await _context.Cart
                    .FirstOrDefaultAsync(ci => ci.IdUser == userId.Value && ci.IdProduct == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Updated quantity for product (ID: {productId}) to {quantity} for user ID: {userId.Value}.");
                }
            }
            else // Anonymous user
            {
                var cart = GetSessionCart();
                var itemToUpdate = cart.FirstOrDefault(item => item.ProductId == productId);
                if (itemToUpdate != null)
                {
                    itemToUpdate.Quantity = quantity;
                    SaveSessionCart(cart);
                    _logger.LogInformation($"Updated quantity for product (ID: {productId}) to {quantity} in session cart.");
                }
            }
        }

        public async Task<int> GetCartItemCountAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                return await _context.Cart
                                 .Where(c => c.IdUser == userId.Value)
                                 .SumAsync(c => c.Quantity);
            }
            return GetSessionCart().Sum(item => item.Quantity);
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                return await _context.Cart
                                 .Where(c => c.IdUser == userId.Value)
                                 .Select(c => c.Quantity * c.Product.Price) // Uses current product price
                                 .SumAsync();
            }
            return GetSessionCart().Sum(item => item.Quantity * item.UnitPrice); // Uses price at time of adding
        }

        public async Task ClearCartAsync()
        {
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var cartItems = await _context.Cart.Where(c => c.IdUser == userId.Value).ToListAsync();
                if (cartItems.Any())
                {
                    _context.Cart.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                Session?.Remove(CartSessionKey);
            }
            _logger.LogInformation($"Cart cleared for user/session.");
        }

        public async Task MigrateCartAsync(int userId)
        {
            // Pobierz koszyk z sesji (koszyk anonimowego użytkownika)
            var sessionCart = GetSessionCart();

            // Jeśli w sesji nie ma żadnych produktów, nie ma czego migrować.
            if (!sessionCart.Any())
            {
                _logger.LogInformation("No session cart to migrate for user ID: {UserId}.", userId);
                return;
            }

            // Pobierz obecny koszyk użytkownika z bazy danych
            var userCart = await _context.Cart
                                     .Where(c => c.IdUser == userId)
                                     .ToListAsync();

            // Przejdź przez każdą pozycję z koszyka sesji
            foreach (var sessionItem in sessionCart)
            {
                // Sprawdź, czy produkt z sesji już istnieje w koszyku w bazie danych
                var dbItem = userCart.FirstOrDefault(c => c.IdProduct == sessionItem.ProductId);

                if (dbItem != null)
                {
                    // Jeśli produkt już jest w koszyku w bazie, złącz ilości (dodaj ilość z sesji)
                    dbItem.Quantity += sessionItem.Quantity;
                }
                else
                {
                    // Jeśli produktu nie ma w koszyku w bazie, dodaj go jako nową pozycję
                    _context.Cart.Add(new Cart
                    {
                        IdUser = userId,
                        IdProduct = sessionItem.ProductId,
                        Quantity = sessionItem.Quantity
                    });
                }
            }

            // Zapisz wszystkie zmiany w bazie danych
            await _context.SaveChangesAsync();

            // Wyczyść koszyk z sesji po pomyślnej migracji
            Session?.Remove(CartSessionKey);

            _logger.LogInformation("Session cart successfully migrated to DB for user ID: {UserId}.", userId);
        }

        #region Helper Methods

        private int? GetCurrentUserId()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }

        private List<CartSessionItem> GetSessionCart()
        {
            var cartJson = Session?.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartSessionItem>();
            }
            try
            {
                return JsonConvert.DeserializeObject<List<CartSessionItem>>(cartJson) ?? new List<CartSessionItem>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing session cart.");
                Session?.Remove(CartSessionKey);
                return new List<CartSessionItem>();
            }
        }

        private void SaveSessionCart(List<CartSessionItem> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            Session?.SetString(CartSessionKey, cartJson);
        }

        public Task MigrateCartAsync(string oldSessionId, int userId)
        {
            throw new NotImplementedException();
        }

        // Helper class for session cart items
        private class CartSessionItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public string? ProductPhotoUrl { get; set; }
        }

        #endregion
    }
}
