using Firma.Data.Data;
using Firma.Data.Data.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Firma.PortalWWW.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly FirmaContext _context;

        public FooterViewComponent(FirmaContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var footerMenuItems = await _context.Page
                .Where(p => p.MenuPlacement == PageMenuPlacement.FooterMenu)
                .OrderBy(p => p.Position)
                .ToListAsync();

            // Przekazuje pobraną listę jako model do widoku komponentu
            return View(footerMenuItems);
        }
    }
}