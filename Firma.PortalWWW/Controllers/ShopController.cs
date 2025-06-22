using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firma.Data.Data;
using Firma.Data.Data.Shop;
using Firma.PortalWWW.Models.Shop;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task<IActionResult> Index(
            int? productTypeId,
            string sortBy = "nameAsc",
            string? searchString = null,
            int pageNumber = 1)
        {
            int pageSize = 9;

            ViewBag.ProductTypes = await _context.ProductType
                                        .OrderBy(pt => pt.Name)
                                        .ToListAsync();

            ViewBag.CurrentProductTypeId = productTypeId;
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchString;

            IQueryable<Product> productsQuery = _context.Product
                                                    .Include(p => p.ProductType);

            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.Description != null && p.Description.Contains(searchString))
                );
            }

            if (productTypeId.HasValue && productTypeId > 0)
            {
                productsQuery = productsQuery.Where(p => p.IdProductType == productTypeId.Value);
                var selectedProductType = await _context.ProductType.FindAsync(productTypeId.Value);
                ViewBag.SelectedProductTypeName = selectedProductType?.Name;
            }

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

            int totalProducts = await productsQuery.CountAsync();
            var products = await productsQuery
                                    .Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.PageSize = pageSize;

            // --- POPRAWIONE MAPOWANIE ---
            var productViewModels = products.Select(p => new ProductViewModel
            {
                Id = p.IdProduct,
                Name = p.Name,
                Price = p.Price,
                //OldPrice = p.OldPrice, 
                Description = p.Description ?? string.Empty,
                ShortDescription = (p.Description != null && p.Description.Length > 100 ? p.Description.Substring(0, 100) + "..." : p.Description) ?? string.Empty,
                ImageUrl = p.PhotoUrl,
                Category = p.ProductType?.Name ?? "Brak kategorii",
                IsInStock = p.IsInStock,
                //IsOnSale = p.OldPrice.HasValue && p.OldPrice > p.Price Logika dla IsOnSale
            }).ToList();

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

            return View(productViewModels);
        }

        // GET: /Shop/Product/5
        public async Task<IActionResult> Product(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productFromDb = await _context.Product
                                    .Include(p => p.ProductType)
                                    .FirstOrDefaultAsync(p => p.IdProduct == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            // --- POPRAWIONE MAPOWANIE ---
            var productViewModel = new ProductViewModel
            {
                Id = productFromDb.IdProduct,
                Name = productFromDb.Name,
                Price = productFromDb.Price,
                Description = productFromDb.Description ?? string.Empty,
                ShortDescription = (productFromDb.Description != null && productFromDb.Description.Length > 100 ? productFromDb.Description.Substring(0, 100) + "..." : productFromDb.Description) ?? string.Empty,
                ImageUrl = productFromDb.PhotoUrl,
                Category = productFromDb.ProductType?.Name ?? "Brak kategorii",
               // OldPrice = productFromDb.OldPrice,
                Rating = 0,         // Domyślnie
                Reviews = 0,        // Domyślnie
                //IsOnSale = productFromDb.OldPrice.HasValue && productFromDb.OldPrice > productFromDb.Price, 
                IsInStock = productFromDb.IsInStock 
            };

            ViewData["Title"] = productViewModel.Name;
            return View(productViewModel);
        }
    }
}