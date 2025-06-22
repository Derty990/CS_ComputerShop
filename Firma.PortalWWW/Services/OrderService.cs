using Firma.Data.Data;
using Firma.Data.Data.Customers;
using Firma.PortalWWW.Interfaces;
using Firma.PortalWWW.Models.Shop;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Firma.PortalWWW.Services
{
    public class OrderService : IOrderService
    {
        private readonly FirmaContext _context;
        private readonly ICartService _cartService;

        public OrderService(FirmaContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<Order?> CreateOrderAsync(CheckoutViewModel checkoutModel, int userId)
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            if (!cartItems.Any())
            {
                return null;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tworzymy obiekt Twojej klasy Order
                var order = new Order
                {
                    IdUser = userId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending, // Używamy Twojego enuma
                    TotalAmount = await _cartService.GetCartTotalAsync(), // Uzupełniamy nowe pole
                    // Uzupełniamy dane do wysyłki z modelu widoku
                    FirstName = checkoutModel.FirstName,
                    LastName = checkoutModel.LastName,
                    Address = checkoutModel.Address,
                    City = checkoutModel.City,
                    PostalCode = checkoutModel.PostalCode,
                    Notes = checkoutModel.Notes
                };

                // Dodajemy do Twojego DbSet<Order>
                _context.Order.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    // Tworzymy obiekt Twojej klasy OrderItem
                    var orderItem = new OrderItem
                    {
                        IdOrder = order.IdOrder, // Używamy klucza głównego z nowo utworzonego zamówienia
                        IdProduct = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    _context.OrderItem.Add(orderItem);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                /*var user = await _context.User.FindAsync(order.IdUser);
                if (user != null)
                {
                    await _email
                        .To(user.Email, user.Name)
                        .Subject($"Potwierdzenie zamówienia #{order.IdOrder}")
                        .Body($"Cześć {user.Name},<br>dziękujemy za złożenie zamówienia nr {order.IdOrder} o wartości {order.TotalAmount:C}.<br>Pozdrawiamy, Zespół Vivitech.")
                        .SendAsync();
                }*/

                // Użycie Twojego serwisu do czyszczenia koszyka
                // Prawdopodobnie będzie trzeba go zmodyfikować, by czyścił koszyk dla danego IdUser
                await _cartService.ClearCartAsync();

                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return null;
            }
        }
    }
}