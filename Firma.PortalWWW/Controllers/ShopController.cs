using Firma.PortalWWW.Models.Shop;
using Microsoft.AspNetCore.Mvc;

namespace Firma.PortalWWW.Controllers
{
    public class ShopController : Controller
    {
        // GET: ShopController
        public ActionResult Index()
        {
            // Tu będze pobierać produkty z bazy danych
            // Na razie tworze przykładowe dane
            var products = GetSampleProducts();
            return View(products);
        }

        public IActionResult Product(int id)
        {
            // Tu będze pobierać konkretny produkt z bazy danych
            // Na razie zwracam przykładowy produkt
            var product = GetSampleProducts().FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        private List<ProductViewModel> GetSampleProducts()
        {
            var products = new List<ProductViewModel>();

            var categories = new[] { "Komputery", "Podzespoły", "Peryferia" };

            var productNames = new[]
            {
                "Laptop Gamingowy XTreme", "Komputer StacjoX Pro", "Procesor UltraCore i9",
                "Karta graficzna RTX 4090", "Monitor 32\" UltraWide", "Klawiatura mechaniczna RGB",
                "Mysz gamingowa bezprzewodowa", "Słuchawki z mikrofonem", "Obudowa komputerowa ATX",
                "Zasilacz modularny 850W", "Płyta główna Z790", "Pamięć RAM 32GB DDR5"
            };

            Random rand = new Random();

            for(int i = 1; i<= 12; i++)
            {
                var price = rand.Next(150, 5000);
                var isOnSale = rand.Next(0, 2) == 1;
                var oldPrice = isOnSale ? price + rand.Next(50, 500) : (decimal?) null;

                products.Add(new ProductViewModel
                {
                    Id = i,
                    Name = productNames[i - 1],
                    Price = price,
                    OldPrice = oldPrice,
                    Description = "Szczegółowy opis produktu. Ten tekst jest dłuższy i zawiera więcej informacji o produkcie.",
                    ShortDescription = "Krótki opis produktu",
                    ImageUrl = "https://placehold.co/600x400/000080/FFFFFF/png",
                    Category = categories[i % categories.Length],
                    Rating = Math.Round(3 + rand.NextDouble() * 2, 1),
                    Reviews = rand.Next(5, 100),
                    IsOnSale = isOnSale,
                    IsInStock = rand.Next(0, 10) > 0



                });


            }


            return products;

        }

        // GET: ShopController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ShopController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ShopController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ShopController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ShopController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ShopController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ShopController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
