using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Firma.Data.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using Firma.PortalWWW.Models; 
using System.Diagnostics;
using Firma.Data.Data.CMS;

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

        // Prywatna metoda pomocnicza do �adowania stron nawigacyjnych dla layoutu
       

        public async Task<IActionResult> Index(int? id) // id to IdPage z CMS.Page
        {
           
            if (id.HasValue) // Je�li przekazano ID, wy�wietlam konkretn� stron� Page
            {
                var pageToShow = await _context.Page.FindAsync(id.Value);
                if (pageToShow != null)
                {
                    ViewBag.SelectedPage = pageToShow; // To b�dzie u�yte w Index.cshtml do wy�wietlenia tre�ci strony
                    ViewData["Title"] = pageToShow.Title;
                    return View(); // Widok Index.cshtml obs�u�y wy�wietlenie tej strony
                }
                _logger.LogWarning($"Nie znaleziono strony o ID: {id.Value}");
                return NotFound();
            }
            else // Wy�wietlam standardow� stron� g��wn�
            {
                ViewData["Title"] = "Strona g��wna";

                // Pobieranie aktualno�ci na stron� g��wn�
                ViewBag.ModelAktualnosci = await _context.Topicality
                                                .OrderByDescending(t => t.IdTopicality)
                                                .Take(3)
                                                .ToListAsync();

                // Pobieranie kategorii produkt�w do sekcji "Kategorie produkt�w"
                ViewBag.FeaturedProductTypes = await _context.ProductType
                                    .Where(pt => pt.IsFeaturedOnHomepage) // Pobieram tylko polecane
                                    .OrderBy(pt => pt.DisplayOrderHomepage) // Sortuje wed�ug kolejno�ci
                                    .ThenBy(pt => pt.Name) // Dodatkowe sortowanie po nazwie
                                    .ToListAsync();

                // Pobieranie produkt�w do sekcji "Polecane produkty"
                ViewBag.FeaturedProducts = await _context.Product
                                 .Where(p => p.IsFeaturedOnHomepage) // Pobieram tylko polecane
                                 .Include(p => p.ProductType) // Do��czenie nazwy kategorii
                                 .OrderBy(p => p.DisplayOrderHomepage) // Sortowanie wed�ug kolejno�ci na SG
                                 .ThenByDescending(p => p.IdProduct) // Dodatkowe sortowanie, np. najnowsze z polecanych
                                 .ToListAsync(); // Mo�na doda� .Take(X), do ograniczenya liczby produtk�w

                return View();
            }
        }
        //TUTAJ DO POPRAWY, CO� JEST �LE, STRONA PRYWATNOSCI MA NIE BY� ZARZ�DZANA DYNAMICZNIE
        public async Task<IActionResult> Privacy()
        {
            
            var privacyPage = await _context.Page
                                    .FirstOrDefaultAsync(p => p.TitleLink.ToLower() == "polityka-prywatnosci");

            if (privacyPage != null)
            {
                ViewBag.SelectedPage = privacyPage; // Przekazuje do Index.cshtml jako wybran� stron�
                ViewData["Title"] = privacyPage.Title;
                return View("Index"); // U�ywam widoku Index.cshtml do wy�wietlenia tre�ci
            }

            // Fallback, je�li strona "Polityka prywatno�ci" nie jest zarz�dzana przez CMS lub nie zosta�a znaleziona
            ViewData["Title"] = "Polityka prywatno�ci";
            return View(); // Zwraca standardowy widok Views/Home/Privacy.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ArticleDetails(int? id)
        {
          
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Topicality.FindAsync(id);

            if (article == null)
            {
                return NotFound();
            }

            ViewData["Title"] = article.Title;
            return View(article); // Przekazuje ca�� encj� Topicality do nowego widoku
        }
    }
}