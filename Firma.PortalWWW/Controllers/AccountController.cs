using Microsoft.AspNetCore.Mvc;
using Firma.Data.Data; 
using Firma.Data.Data.Customers;
using Firma.PortalWWW.Models.Account; 
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; 
using Microsoft.AspNetCore.Authorization;
using Firma.PortalWWW.Interfaces;
using Firma.PortalWWW.Services;
using Firma.PortalWWW.Documents;
using QuestPDF.Fluent;
// Do hashowania haseł użyć biblioteki takiej jak BCrypt.Net-Next
// instalacja przez NuGet: Install-Package BCrypt.Net-Next
// Lub można użyć wbudowanych mechanizmów ASP.NET Core Identity

namespace Firma.PortalWWW.Controllers
{
    public class AccountController : Controller
    {
        private readonly FirmaContext _context;
        private readonly ICartService _cartService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(FirmaContext context, ICartService cartService, ILogger<AccountController> logger)
        {
            _context = context;
            _cartService = cartService;
            _logger = logger;
        }

        [Authorize] // Tylko zalogowani użytkownicy mogą wejść do swojego panelu
        public IActionResult Index()
        {
            ViewData["Title"] = "Panel Klienta";
            return View();
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Sprawdź, czy użytkownik o takim emailu już nie istnieje
                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Użytkownik o podanym adresie email już istnieje.");
                    return View(model);
                }

                var user = new User
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    // Haszowanie hasła - BARDZO WAŻNE!
                    HashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RegistrationDate = DateTime.UtcNow,
                    Role = UserRole.User // Domyślnie każdy nowy użytkownik to 'User'
                };

                _context.User.Add(user);
                await _context.SaveChangesAsync();

                _logger?.LogInformation($"Użytkownik {user.Email} został zarejestrowany.");

                // Opcjonalnie: automatyczne logowanie po rejestracji
                // await SignInUser(user); 
                // return RedirectToLocal(returnUrl);

                // Przekierowanie do strony logowania z komunikatem o sukcesie
                TempData["SuccessMessage"] = "Rejestracja zakończona pomyślnie. Możesz się teraz zalogować.";
                return RedirectToAction(nameof(Login));
            }
            // Jeśli coś poszło nie tak, wyświetl formularz ponownie
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);

                // Sprawdź, czy użytkownik istnieje i czy hasło jest poprawne
                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.HashedPassword))
                {
                    await SignInUser(user, model.RememberMe); // Metoda pomocnicza do logowania

                    await _cartService.MigrateCartAsync(user.IdUser); // Migracja koszyka z sesji do bazy

                    _logger?.LogInformation($"Użytkownik {user.Email} zalogował się.");
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Nieprawidłowa próba logowania. Sprawdź email i hasło.");
            }
            // Jeśli coś poszło nie tak, wyświetl formularz ponownie
            return View(model);
        }

        // Metoda pomocnicza do logowania użytkownika (tworzenia ciasteczka)
        private async Task SignInUser(User user, bool isPersistent = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Dodaje rolę użytkownika
                // Można dodać więcej claims, jeśli trzeba
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent, // Jeśli "RememberMe" jest zaznaczone, ciasteczko będzie trwałe
                // Można ustawić inne właściwości, np. czas wygaśnięcia
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20) 
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [Authorize] // Upewnij się, że tylko zalogowani użytkownicy mogą się wylogować
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger?.LogInformation("Użytkownik wylogował się.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        // Metoda pomocnicza do bezpiecznego przekierowania
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        // Można dodać akcję AccessDenied, jeśli używa się autoryzacji opartej na rolach
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize] // Upewniam się, że tylko zalogowani użytkownicy mogą zobaczyć swoje zamówienia
        public async Task<IActionResult> Orders()
        {
            // Pobieram ID zalogowanego użytkownika z jego "dowodu tożsamości" (ciasteczka)
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Pobieram z bazy wszystkie zamówienia dla tego użytkownika,
            // sortując je od najnowszego do najstarszego.
            var orders = await _context.Order
                                       .Where(o => o.IdUser == userId)
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToListAsync();

            ViewData["Title"] = "Moje zamówienia";
            return View(orders);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            // Pobieram z bazy jedno konkretne zamówienie.
            // Używam Include(), aby od razu dociągnąć powiązane z nim pozycje zamówienia (OrderItems),
            // a dla każdej pozycji także dane produktu (Product).
            // BARDZO WAŻNE: Dodatkowo sprawdzam, czy zamówienie należy do zalogowanego użytkownika!
            var order = await _context.Order
                                      .Include(o => o.OrderItems)
                                      .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.IdOrder == id && o.IdUser == userId);

            if (order == null)
            {
                // Jeśli zamówienie nie istnieje lub nie należy do tego użytkownika, zwracam błąd 404.
                return NotFound();
            }

            ViewData["Title"] = $"Szczegóły zamówienia #{order.IdOrder}";
            return View(order);
        }

        [Authorize]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var order = await _context.Order
                                      .Include(o => o.OrderItems)
                                      .ThenInclude(oi => oi.Product)
                                      .FirstOrDefaultAsync(o => o.IdOrder == id && o.IdUser == userId);

            if (order == null)
            {
                return NotFound();
            }

            // Tworzę dokument na podstawie mojego szablonu i danych zamówienia
            var document = new InvoiceDocument(order);
            // Generuję plik PDF do pamięci
            byte[] pdfBytes = document.GeneratePdf();

            // Zwracam plik do przeglądarki
            return File(pdfBytes, "application/pdf", $"faktura-nr-{order.IdOrder}.pdf");
        }
    }
}