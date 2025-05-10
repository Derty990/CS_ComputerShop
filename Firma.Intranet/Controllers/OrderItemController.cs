using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Firma.Data.Data; 
using Firma.Data.Data.Customers; 
using Firma.Data.Data.Shop;    

namespace Firma.Intranet.Controllers
{
    public class OrderItemController : Controller
    {
        private readonly FirmaContext _context;

        public OrderItemController(FirmaContext context)
        {
            // Wstrzykuję kontekst bazy danych przy tworzeniu kontrolera
            _context = context;
        }

        // Metoda pomocnicza do ładowania list rozwijanych dla formularzy
        private void PopulateDropdowns(OrderItem? orderItem = null)
        {
            // Przygotowuję listę dla Zamówień z bardziej opisowym tekstem
            var ordersQuery = _context.Order
                                    .Include(o => o.User) // Dołączam dane użytkownika
                                    .OrderByDescending(o => o.OrderDate); // Sortuję po dacie

            // Tworzę listę obiektów do wyświetlenia w SelectList
            var orderDisplayList = ordersQuery.Select(o => new
            {
                Id = o.IdOrder,
                // Formatuję tekst jako "Zam. #ID (Imię Nazwisko - Data)"
                Text = $"Zam. #{o.IdOrder} ({o.User.Name} - {o.OrderDate:dd.MM.yyyy})"
            }).ToList();

            // Przypisuję SelectList do ViewData, zaznaczając ew. bieżącą wartość
            ViewData["IdOrder"] = new SelectList(orderDisplayList, "Id", "Text", orderItem?.IdOrder);

            // Przygotowuję listę dla Produktów, używając nazwy produktu
            ViewData["IdProduct"] = new SelectList(_context.Product.OrderBy(p => p.Name), "IdProduct", "Name", orderItem?.IdProduct);
        }


        // GET: OrderItem lub OrderItem?orderId=X
        // Wyświetlam listę pozycji zamówień, opcjonalnie filtrowaną dla konkretnego zamówienia
        public async Task<IActionResult> Index(int? orderId)
        {
            // Tworzę bazowe zapytanie, dołączając dane z powiązanych tabel Order (z User) i Product
            var query = _context.OrderItem
                                .Include(o => o.Order)
                                    .ThenInclude(o => o.User) // Dołączam User przez Order
                                .Include(o => o.Product)
                                .AsQueryable(); // Używam AsQueryable, aby móc dynamicznie dodawać warunki

            // Jeśli przekazano orderId, filtruję wyniki
            if (orderId.HasValue)
            {
                query = query.Where(oi => oi.IdOrder == orderId.Value);

                // Przekazuję informację o filtrowaniu do widoku dla lepszego UX
                var order = await _context.Order.Include(o => o.User).FirstOrDefaultAsync(o => o.IdOrder == orderId.Value);
                ViewBag.OrderFilterInfo = order != null ? $"dla zamówienia #{order.IdOrder} ({order.User?.Name})" : "";
                ViewBag.OrderId = orderId.Value; // Przekazuję ID zamówienia do widoku (np. dla przycisku powrotu)
            }

            // Zwracam widok z listą pozycji (przefiltrowaną lub pełną)
            return View(await query.ToListAsync());
        }

        // GET: OrderItem/Details/5 - Akcja pozostaje, ale widok nie jest tworzony na życzenie
        // Wyświetlam szczegóły pojedynczej pozycji zamówienia
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Nie znaleziono ID
            }

            // Pobieram pozycję z bazy, dołączając powiązane dane
            var orderItem = await _context.OrderItem
                .Include(o => o.Order)
                    .ThenInclude(o => o.User) // Dołączam też User przez Order
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.IdOrderItem == id);

            if (orderItem == null)
            {
                return NotFound(); // Nie znaleziono pozycji o danym ID
            }

            // Zwracam widok ze szczegółami (nawet jeśli plik .cshtml nie istnieje, akcja jest poprawna)
            return View(orderItem);
        }

        // GET: OrderItem/Create lub OrderItem/Create?orderId=X
        // Wyświetlam formularz tworzenia nowej pozycji zamówienia
        public IActionResult Create(int? orderId)
        {
            var orderItem = new OrderItem(); // Tworzę pusty model dla formularza

            // Jeśli orderId zostało przekazane (np. z widoku edycji zamówienia), ustawiam je w modelu
            if (orderId.HasValue)
            {
                orderItem.IdOrder = orderId.Value;
                // Wstępnie zaznaczam odpowiednie zamówienie na liście rozwijanej (przekazuję orderId do PopulateDropdowns)
                PopulateDropdowns(orderItem);
                // Poniższa linia nie jest już potrzebna, bo PopulateDropdowns obsługuje zaznaczenie
                // ViewData["IdOrder"] = new SelectList(((SelectList)ViewData["IdOrder"]).Items, "Value", "Text", orderId.Value);
            }
            else
            {
                // Jeśli orderId nie jest przekazane, po prostu ładuję listy rozwijane
                PopulateDropdowns();
            }


            // Zwracam widok z formularzem
            return View(orderItem);
        }

        // POST: OrderItem/Create
        // Przetwarzam dane z formularza tworzenia nowej pozycji
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrder,IdProduct,Quantity,UnitPrice")] OrderItem orderItem)
        {
            // Opcjonalnie: Weryfikacja/ustawienie UnitPrice na podstawie produktu
            var product = await _context.Product.FindAsync(orderItem.IdProduct);
            if (product == null)
            {
                ModelState.AddModelError("IdProduct", "Wybrany produkt nie istnieje.");
            }
            // else { /* Można ustawić: orderItem.UnitPrice = product.Price; */ }

            // Sprawdzam, czy model przeszedł walidację
            if (ModelState.IsValid)
            {
                // Dodaję nową pozycję do kontekstu
                _context.Add(orderItem);
                // Zapisuję zmiany w bazie danych
                await _context.SaveChangesAsync();
                // Przekierowuję użytkownika z powrotem do strony edycji zamówienia, do którego dodano pozycję
                return RedirectToAction("Edit", "Order", new { id = orderItem.IdOrder });
            }

            // Jeśli model nie jest poprawny, ponownie ładuję listy rozwijane (z zachowaniem wybranych wartości)
            PopulateDropdowns(orderItem);
            // Zwracam widok z formularzem i błędami walidacji
            return View(orderItem);
        }

        // GET: OrderItem/Edit/5
        // Wyświetlam formularz edycji istniejącej pozycji zamówienia
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Znajduję pozycję w bazie danych
            var orderItem = await _context.OrderItem.FindAsync(id); // FindAsync nie wymaga Include, ale przydałoby się do wyświetlenia w widoku
            // Lepsze byłoby:
            // var orderItem = await _context.OrderItem
            //                         .Include(oi => oi.Order).ThenInclude(o => o.User)
            //                         .Include(oi => oi.Product)
            //                         .FirstOrDefaultAsync(oi => oi.IdOrderItem == id);


            if (orderItem == null)
            {
                return NotFound(); // Nie znaleziono pozycji
            }

            // Ładuję listy rozwijane z zaznaczonymi aktualnymi wartościami
            PopulateDropdowns(orderItem);
            // Zwracam widok z formularzem edycji
            return View(orderItem);
        }

        // POST: OrderItem/Edit/5
        // Przetwarzam dane z formularza edycji pozycji
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOrderItem,IdOrder,IdProduct,Quantity,UnitPrice")] OrderItem orderItem)
        {
            // Sprawdzam, czy ID z URL zgadza się z ID w modelu
            if (id != orderItem.IdOrderItem)
            {
                return NotFound();
            }

            // Opcjonalnie: Ponowna weryfikacja produktu i ceny
            var product = await _context.Product.FindAsync(orderItem.IdProduct);
            if (product == null)
            {
                ModelState.AddModelError("IdProduct", "Wybrany produkt nie istnieje.");
            }

            // Sprawdzam, czy model jest poprawny
            if (ModelState.IsValid)
            {
                try
                {
                    // Aktualizuję dane pozycji w kontekście
                    _context.Update(orderItem);
                    // Zapisuję zmiany w bazie
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Obsługuję błąd współbieżności (jeśli inny użytkownik zmodyfikował dane w międzyczasie)
                    if (!OrderItemExists(orderItem.IdOrderItem))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Rzucam dalej wyjątek, jeśli nie wiem, jak go obsłużyć
                    }
                }
                // Przekierowuję z powrotem do strony edycji zamówienia nadrzędnego
                return RedirectToAction("Edit", "Order", new { id = orderItem.IdOrder });
            }

            // Jeśli model nie jest poprawny, ponownie ładuję listy rozwijane
            PopulateDropdowns(orderItem);
            // Zwracam widok z formularzem i błędami walidacji
            return View(orderItem);
        }

        // GET: OrderItem/Delete/5
        // Wyświetlam potwierdzenie usunięcia pozycji (choć widok nie będzie tworzony)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Pobieram pozycję do usunięcia wraz z powiązanymi danymi (dla ew. wyświetlenia w przyszłym widoku)
            var orderItem = await _context.OrderItem
                .Include(o => o.Order)
                    .ThenInclude(o => o.User)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.IdOrderItem == id);

            if (orderItem == null)
            {
                return NotFound();
            }

            // Zwracam widok (który może nie istnieć, ale akcja jest gotowa)
            return View(orderItem);
        }

        // POST: OrderItem/Delete/5
        // Potwierdzam i wykonuję usunięcie pozycji
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Znajduję pozycję do usunięcia
            var orderItem = await _context.OrderItem.FindAsync(id);
            int? orderId = orderItem?.IdOrder; // Zapamiętuję IdOrder przed usunięciem, aby wiedzieć gdzie wrócić

            if (orderItem != null)
            {
                // Usuwam pozycję z kontekstu
                _context.OrderItem.Remove(orderItem);
                // Zapisuję zmiany w bazie
                await _context.SaveChangesAsync();
            }

            // Przekierowuję z powrotem do edycji zamówienia nadrzędnego (jeśli udało się zapamiętać IdOrder)
            // lub do listy pozycji (jeśli coś poszło nie tak)
            if (orderId.HasValue)
            {
                return RedirectToAction("Edit", "Order", new { id = orderId.Value });
            }
            return RedirectToAction(nameof(Index)); // Fallback
        }

        // Prywatna metoda sprawdzająca, czy pozycja o danym ID istnieje w bazie
        private bool OrderItemExists(int id)
        {
            return _context.OrderItem.Any(e => e.IdOrderItem == id);
        }
    }
}