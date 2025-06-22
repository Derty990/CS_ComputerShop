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

        // Prywatna metoda pomocnicza do ≥adowania stron nawigacyjnych dla layoutu
       

        public async Task<IActionResult> Index(int? id) // id to IdPage z CMS.Page
        {
           
            if (id.HasValue) // Jeúli przekazano ID, wyúwietlam konkretnπ stronÍ Page
            {
                var pageToShow = await _context.Page.FindAsync(id.Value);
                if (pageToShow != null)
                {
                    ViewBag.SelectedPage = pageToShow; // To bÍdzie uøyte w Index.cshtml do wyúwietlenia treúci strony
                    ViewData["Title"] = pageToShow.Title;
                    return View(); // Widok Index.cshtml obs≥uøy wyúwietlenie tej strony
                }
                _logger.LogWarning($"Nie znaleziono strony o ID: {id.Value}");
                return NotFound();
            }
            else // Wyúwietlam standardowπ stronÍ g≥Ûwnπ
            {
                ViewData["Title"] = "Strona g≥Ûwna";

                // Pobieranie aktualnoúci na stronÍ g≥Ûwnπ
                ViewBag.ModelAktualnosci = await _context.Topicality
                                                .OrderByDescending(t => t.IdTopicality)
                                                .Take(3)
                                                .ToListAsync();

                // Pobieranie kategorii produktÛw do sekcji "Kategorie produktÛw"
                ViewBag.FeaturedProductTypes = await _context.ProductType
                                    .Where(pt => pt.IsFeaturedOnHomepage) // Pobieram tylko polecane
                                    .OrderBy(pt => pt.DisplayOrderHomepage) // Sortuje wed≥ug kolejnoúci
                                    .ThenBy(pt => pt.Name) // Dodatkowe sortowanie po nazwie
                                    .ToListAsync();

                // Pobieranie produktÛw do sekcji "Polecane produkty"
                ViewBag.FeaturedProducts = await _context.Product
                                 .Where(p => p.IsFeaturedOnHomepage) // Pobieram tylko polecane
                                 .Include(p => p.ProductType) // Do≥πczenie nazwy kategorii
                                 .OrderBy(p => p.DisplayOrderHomepage) // Sortowanie wed≥ug kolejnoúci na SG
                                 .ThenByDescending(p => p.IdProduct) // Dodatkowe sortowanie, np. najnowsze z polecanych
                                 .ToListAsync(); // Moøna dodaÊ .Take(X), do ograniczenya liczby produtkÛw

                return View();
            }
        }
        //TUTAJ DO POPRAWY, COå JEST èLE, STRONA PRYWATNOSCI MA NIE BY∆ ZARZ•DZANA DYNAMICZNIE
        public async Task<IActionResult> Privacy()
        {
            
            var privacyPage = await _context.Page
                                    .FirstOrDefaultAsync(p => p.TitleLink.ToLower() == "polityka-prywatnosci");

            if (privacyPage != null)
            {
                ViewBag.SelectedPage = privacyPage; // Przekazuje do Index.cshtml jako wybranπ stronÍ
                ViewData["Title"] = privacyPage.Title;
                return View("Index"); // Uøywam widoku Index.cshtml do wyúwietlenia treúci
            }

            // Fallback, jeúli strona "Polityka prywatnoúci" nie jest zarzπdzana przez CMS lub nie zosta≥a znaleziona
            ViewData["Title"] = "Polityka prywatnoúci";
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
            return View(article); // Przekazuje ca≥π encjÍ Topicality do nowego widoku
        }
    }
}