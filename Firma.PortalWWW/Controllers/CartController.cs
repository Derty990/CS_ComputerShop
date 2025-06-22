using Microsoft.AspNetCore.Mvc;
using Firma.PortalWWW.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Firma.PortalWWW.Interfaces;

namespace Firma.PortalWWW.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        // GET: /Cart
        //Wyświetla główną stronę koszyka
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            ViewBag.CartTotal = await _cartService.GetCartTotalAsync();
            ViewData["Title"] = "Twój koszyk";

            return View(cartItems);
        }

        // POST: /Cart/AddToCart
        // Obsługuje dodawanie do koszyka
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            if (productId <= 0 || quantity <= 0)
            {
                TempData["CartErrorMessage"] = "Niepoprawne dane o produkcie.";
                return RedirectToAction("Index", "Shop");
            }

            await _cartService.AddToCartAsync(productId, quantity);
            TempData["CartSuccessMessage"] = "Produkt został dodany do twojego koszyka!";
            _logger.LogInformation("Added product {ProductId} with quantity {Quantity} to cart.", productId, quantity);

            /*// Przekieruj z powrotem do strony z której przyszedł użytkownik
            string referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return Redirect(referer);
            }*/

            // Sprawdzam, czy przekazano bezpieczny adres powrotu.
            if (Url.IsLocalUrl(returnUrl))
            {
                // Jeśli tak, wracam pod ten adres.
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Shop");
        }

        // POST: /Cart/RemoveItem
        // Obsługuje usuwanie całej jednej "linii" produktu z koszyka
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            if (productId > 0)
            {
                await _cartService.RemoveFromCartAsync(productId);
                TempData["CartUpdateMessage"] = "Product has been removed from your cart.";
                _logger.LogInformation("Removed product {ProductId} from cart.", productId);
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        // Obsługuje aktualizoanie ilości danego produktu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            // The service handles removing the item if quantity is 0 or less
            if (productId > 0)
            {
                await _cartService.UpdateQuantityAsync(productId, quantity);
                TempData["CartUpdateMessage"] = "Koszyk został zaktualizowany.";
                _logger.LogInformation("Updated quantity for product {ProductId} to {Quantity}.", productId, quantity);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/GetCartSummary
        // Akcja do wywołania przez javascript do aktalizacji ikony koszyka w layoucie
        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var itemCount = await _cartService.GetCartItemCountAsync();
            return Json(new { itemCount = itemCount });
        }
    }
}