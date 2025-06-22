using Firma.Data.Data;
using Firma.PortalWWW.Interfaces;
using Firma.PortalWWW.Models.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; 
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentEmail.Core;

namespace Firma.PortalWWW.Controllers
{
    [Authorize] // Ten atrybut jest poprawny i musi tu zostać
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly FirmaContext _context;
        private readonly IFluentEmail _email;

        public CheckoutController(ICartService cartService, IOrderService orderService, FirmaContext context, IFluentEmail email)
        {
            _cartService = cartService;
            _orderService = orderService;
            _context = context;
            _email = email;
        }

        // GET: /Checkout/Index
        public async Task<IActionResult> Index()
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            if (cartItems == null || !cartItems.Any())
            {
                TempData["CartErrorMessage"] = "Twój koszyk jest pusty. Nie możesz przejść do podsumowania.";
                return RedirectToAction("Index", "Cart");
            }

            // POPRAWNY SPOSÓB POBIERANIA DANYCH ZALOGOWANEGO UŻYTKOWNIKA
            var userName = User.Identity.Name;
            var (firstName, lastName) = SplitFullName(userName);

            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                CartTotal = await _cartService.GetCartTotalAsync(),
                FirstName = firstName,
                LastName = lastName,
            };

            ViewData["Title"] = "Podsumowanie zamówienia";
            return View(model);
        }

        // POST: /Checkout/ProcessOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(CheckoutViewModel model)
        {
            // POPRAWNY SPOSÓB POBIERANIA ID UŻYTKOWNIKA
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                // To nie powinno się zdarzyć dla autoryzowanego użytkownika, ale jest to dobre zabezpieczenie
                return Unauthorized("Nie można zidentyfikować użytkownika.");
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = await _cartService.GetCartItemsAsync();
                model.CartTotal = await _cartService.GetCartTotalAsync();
                return View("Index", model);
            }

            var createdOrder = await _orderService.CreateOrderAsync(model, userId);

            if (createdOrder != null)
            {
                try
                {
                    var userEmail = User.FindFirstValue(ClaimTypes.Email);
                    var userName = User.FindFirstValue(ClaimTypes.Name);

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var emailToSend = _email
                            .To(userEmail, userName)
                            .Subject($"Potwierdzenie zamówienia #{createdOrder.IdOrder}")
                            .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Views/Emails/_OrderConfirmation.cshtml", createdOrder);

                        // 1. Przechwytuję odpowiedź z serwera do zmiennej 'response'
                        var response = await emailToSend.SendAsync();

                        // 2. POSTAW BREAKPOINT NA PONIŻSZEJ LINII
                        // i po zatrzymaniu, najedź kursorem na zmienną 'response', aby zobaczyć jej zawartość.
                        // interesują nas zwłaszcza pola: response.Successful i response.ErrorMessages
                    }
                }
                catch (Exception ex)
                {
                    // Ten blok zostaje na wszelki wypadek, gdyby Mailtrap faktycznie miał problem.
                    // Możesz tu dodać logowanie błędu, aby śledzić takie sytuacje.
                    // _logger.LogError(ex, "Błąd wysyłania e-maila z potwierdzeniem zamówienia.");
                }

                // Niezależnie od tego, czy e-mail się udał, przekierowuję na stronę sukcesu.
                return RedirectToAction("Confirmation", new { orderId = createdOrder.IdOrder });
            }

            // Jeśli zamówienie z jakiegoś powodu nie powstało
            TempData["OrderErrorMessage"] = "Wystąpił błąd podczas składania zamówienia. Spróbuj ponownie.";
            return RedirectToAction("Index");
        }

        // GET: /Checkout/Confirmation/5
        public IActionResult Confirmation(int orderId)
        {
            ViewData["Title"] = "Potwierdzenie zamówienia";
            ViewBag.OrderId = orderId;
            return View();
        }

        // Funkcja pomocnicza do podziału imienia i nazwiska
        private (string, string) SplitFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return (string.Empty, string.Empty);
            }
            var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : string.Empty;
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;
            return (firstName, lastName);
        }
    }
}