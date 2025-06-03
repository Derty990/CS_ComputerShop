using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firma.Data.Data; // Dostęp do FirmaContext
using Firma.Data.Data.Shop; // Dostęp do Product, ProductType
using Firma.PortalWWW.Models.Shop; // Dla ProductViewModel
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // Dla List

namespace Firma.PortalWWW.Controllers
{
    public class ShopController : Controller
    {
        private readonly FirmaContext _context;

        public ShopController(FirmaContext context)
        {
            _context = context;
        }

        // GET: /Shop lub /Shop/Index?productTypeId=X&sortBy=Y&searchString=Z
        public async Task<IActionResult> Index(int? productTypeId, string sortBy = "nameAsc", string? searchString = null)
        {
            // Pobieranie wszystkich typów produktów do wyświetlenia w menu/filtrach
            ViewBag.ProductTypes = await _context.ProductType
                                            .OrderBy(pt => pt.Name)
                                            .ToListAsync();

            // Przekazanie aktualnych wartości filtrów i sortowania do widoku, aby można je było utrzymać w UI
            ViewBag.CurrentProductTypeId = productTypeId;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchString;

            // Tworzenie bazowego zapytania dla produktów
            IQueryable<Product> productsQuery = _context.Product
                                                    .Include(p => p.ProductType); // Dołączenie danych typu produktu (kategorii)

            // Filtrowanie po nazwie lub opisie produktu
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.Description != null && p.Description.Contains(searchString))
                );
            }

            // Filtrowanie po typie produktu (kategorii)
            if (productTypeId.HasValue && productTypeId > 0)
            {
                productsQuery = productsQuery.Where(p => p.IdProductType == productTypeId.Value);
                var selectedProductType = await _context.ProductType.FindAsync(productTypeId.Value);
                ViewBag.SelectedProductTypeName = selectedProductType?.Name; // Nazwa wybranej kategorii do wyświetlenia
            }

            // Sortowanie produktów
            switch (sortBy)
            {
                case "priceAsc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "priceDesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                case "nameDesc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Name);
                    break;
                case "nameAsc":
                default:
                    productsQuery = productsQuery.OrderBy(p => p.Name);
                    break;
            }

            var productsFromDb = await productsQuery.ToListAsync();

            // Mapowanie encji Product z bazy na ProductViewModel
            // Upewnij się, że Twój ProductViewModel ma wszystkie potrzebne pola.
            var productViewModels = productsFromDb.Select(p => new ProductViewModel
            {
                Id = p.IdProduct,
                Name = p.Name,
                Price = p.Price,
              
                OldPrice = null,
                Description = p.Description ?? string.Empty, 
                
                ShortDescription = (p.Description != null && p.Description.Length > 100 ? p.Description.Substring(0, 100) + "..." : p.Description) ?? string.Empty,
                ImageUrl = p.PhotoUrl, 
                Category = p.ProductType?.Name ?? "Brak kategorii",
                CategoryId = p.IdProductType,

               
                Rating = 0,        
                Reviews = 0,       
                IsOnSale = false,   
                IsInStock = true   
            }).ToList();

            // Ustawienie tytułu strony
            if (productTypeId.HasValue && !string.IsNullOrEmpty(ViewBag.SelectedProductTypeName))
            {
                ViewData["Title"] = $"Sklep - {ViewBag.SelectedProductTypeName}";
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                ViewData["Title"] = $"Wyniki wyszukiwania dla: \"{searchString}\"";
            }
            else
            {
                ViewData["Title"] = "Sklep - Wszystkie produkty";
            }

            return View(productViewModels); // Przekazujemy listę ProductViewModel do widoku
        }

        // GET: /Shop/Product/5
        public async Task<IActionResult> Product(int? id)
        {
            if (id == null)
            {
                // Można też przekierować do Index, jeśli ID nie jest podane
                // return RedirectToAction(nameof(Index)); 
                return NotFound();
            }

            var productFromDb = await _context.Product
                                    .Include(p => p.ProductType) // Dołączenie informacji o kategorii
                                    .FirstOrDefaultAsync(p => p.IdProduct == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            // Mapowanie encji Product na ProductViewModel dla strony szczegółów
            var productViewModel = new ProductViewModel
            {
                Id = productFromDb.IdProduct,
                Name = productFromDb.Name,
                Price = productFromDb.Price,
                Description = productFromDb.Description ?? string.Empty,
                ShortDescription = (productFromDb.Description != null && productFromDb.Description.Length > 100 ? productFromDb.Description.Substring(0, 100) + "..." : productFromDb.Description) ?? string.Empty,
                ImageUrl = productFromDb.PhotoUrl,
                Category = productFromDb.ProductType?.Name ?? "Brak kategorii",
                OldPrice = null,      
                Rating = 0,        
                Reviews = 0,        
                IsOnSale = false,   
                IsInStock = true   
            };

            ViewData["Title"] = productViewModel.Name; // Ustawienie tytułu strony na nazwę produktu
            return View(productViewModel); // Przekazujemy ProductViewModel do widoku
        }
    }
}