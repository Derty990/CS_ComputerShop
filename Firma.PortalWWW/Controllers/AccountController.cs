using Firma.PortalWWW.Models.Account;
using Microsoft.AspNetCore.Mvc;

namespace Firma.PortalWWW.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Tutaj będzie logika logowania użytkownika
                // Na razie tylko przekierowanie na stronę główną
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tutaj będzie logika rejestracji użytkownika
                // Na razie tylko przekierowanie na stronę logowania
                return RedirectToAction(nameof(Login));
            }
            return View(model);
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Tutaj będzie logika wylogowania użytkownika
            return RedirectToAction("Index", "Home");
        }
    }
}
