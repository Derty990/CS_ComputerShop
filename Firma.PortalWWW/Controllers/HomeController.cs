using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Firma.Data.Data; // Dostêp do FirmaContext
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Firma.PortalWWW.Models; // Dla ErrorViewModel
using System.Diagnostics;
using Firma.Data.Data.CMS; // Potrzebne dla PageMenuPlacement i Page

namespace Firma.PortalWWW.Controllers
{
    public class HomeController : Controller
    {
        private readonly FirmaContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(FirmaContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Prywatna metoda pomocnicza do ³adowania stron nawigacyjnych dla layoutu
        private async Task PopulateLayoutNavPages()
        {
            ViewBag.MainMenuPages = await _context.Page
                                        .Where(p => p.MenuPlacement == PageMenuPlacement.MainMenu)
                                        .OrderBy(p => p.Position)
                                        .ToListAsync();

            ViewBag.FooterMenuPages = await _context.Page
                                        .Where(p => p.MenuPlacement == PageMenuPlacement.FooterMenu)
                                        .OrderBy(p => p.Position)
                                        .ToListAsync();
        }

        public async Task<IActionResult> Index(int? id) // id to IdPage z CMS.Page
        {
            await PopulateLayoutNavPages(); // £adujemy strony do menu i stopki dla layoutu

            if (id.HasValue) // Jeœli przekazano ID, wyœwietlamy konkretn¹ stronê Page
            {
                var pageToShow = await _context.Page.FindAsync(id.Value);
                if (pageToShow != null)
                {
                    ViewBag.SelectedPage = pageToShow; // To bêdzie u¿yte w Index.cshtml do wyœwietlenia treœci strony
                    ViewData["Title"] = pageToShow.Title;
                    return View(); // Widok Index.cshtml obs³u¿y wyœwietlenie tej strony
                }
                _logger.LogWarning($"Nie znaleziono strony o ID: {id.Value}");
                return NotFound();
            }
            else // Wyœwietlamy standardow¹ stronê g³ówn¹
            {
                ViewData["Title"] = "Strona g³ówna";

                // Pobieranie aktualnoœci na stronê g³ówn¹
                ViewBag.ModelAktualnosci = await _context.Topicality
                                                .OrderByDescending(t => t.IdTopicality)
                                                .Take(3)
                                                .ToListAsync();

                // Pobieranie kategorii produktów do sekcji "Kategorie produktów"
                ViewBag.FeaturedProductTypes = await _context.ProductType
                                        .OrderBy(pt => pt.Name) 
                                        .ToListAsync();

                // Pobieranie produktów do sekcji "Polecane produkty"
                ViewBag.FeaturedProducts = await _context.Product
                                                    .Include(p => p.ProductType) // TO JEST POPRAWNE, bo ProductType to encja powi¹zana
                                                    .OrderByDescending(p => p.IdProduct)
                                                    .Take(4)
                                                    .ToListAsync();
                return View();
            }
        }

        public async Task<IActionResult> Privacy()
        {
            await PopulateLayoutNavPages();

            // Logika dla strony Prywatnoœæ - mo¿e byæ zarz¹dzana przez CMS
            // Ustaw unikalny TitleLink (np. "polityka-prywatnosci") dla strony Polityka Prywatnoœci w Intranecie
            // i oznacz j¹ jako MenuPlacement.FooterMenu
            var privacyPage = await _context.Page
                                    .FirstOrDefaultAsync(p => p.TitleLink.ToLower() == "polityka-prywatnosci");

            if (privacyPage != null)
            {
                ViewBag.SelectedPage = privacyPage; // Przekazujemy do Index.cshtml jako wybran¹ stronê
                ViewData["Title"] = privacyPage.Title;
                return View("Index"); // U¿ywamy widoku Index.cshtml do wyœwietlenia treœci
            }

            // Fallback, jeœli strona "Polityka prywatnoœci" nie jest zarz¹dzana przez CMS lub nie zosta³a znaleziona
            ViewData["Title"] = "Polityka prywatnoœci";
            return View(); // Zwraca standardowy widok Views/Home/Privacy.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            await PopulateLayoutNavPages();
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}