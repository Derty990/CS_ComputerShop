using Firma.Data.Data.Customers;
using Firma.PortalWWW.Models.Shop;
using System.Threading.Tasks;

namespace Firma.PortalWWW.Interfaces
{
    public interface IOrderService
    {
        // Metoda przyjmuje teraz int jako ID użytkownika
        Task<Order?> CreateOrderAsync(CheckoutViewModel checkoutModel, int userId);
    }
}