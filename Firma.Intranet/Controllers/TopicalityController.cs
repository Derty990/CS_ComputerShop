using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Firma.Data.Data.CMS;
using Firma.Data.Data;

namespace Firma.Intranet.Controllers
{
    public class TopicalityController : Controller
    {
        private readonly FirmaContext _context;

        public TopicalityController(FirmaContext context)
        {
            _context = context;
        }

        // GET: Topicality
        public async Task<IActionResult> Index()
        {
            return View(await _context.Topicality.ToListAsync());
        }

        // GET: Topicality/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicality = await _context.Topicality
                .FirstOrDefaultAsync(m => m.IdTopicality == id);
            if (topicality == null)
            {
                return NotFound();
            }

            return View(topicality);
        }

        // GET: Topicality/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Topicality/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTopicality,TitleLink,Title,Contents,Position")] Topicality topicality)
        {
            if (ModelState.IsValid)
            {
                _context.Add(topicality);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(topicality);
        }

        // GET: Topicality/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicality = await _context.Topicality.FindAsync(id);
            if (topicality == null)
            {
                return NotFound();
            }
            return View(topicality);
        }

        // POST: Topicality/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTopicality,TitleLink,Title,Contents,Position")] Topicality topicality)
        {
            if (id != topicality.IdTopicality)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(topicality);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TopicalityExists(topicality.IdTopicality))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(topicality);
        }

        // GET: Topicality/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topicality = await _context.Topicality
                .FirstOrDefaultAsync(m => m.IdTopicality == id);
            if (topicality == null)
            {
                return NotFound();
            }

            return View(topicality);
        }

        // POST: Topicality/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var topicality = await _context.Topicality.FindAsync(id);
            if (topicality != null)
            {
                _context.Topicality.Remove(topicality);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TopicalityExists(int id)
        {
            return _context.Topicality.Any(e => e.IdTopicality == id);
        }
    }
}
