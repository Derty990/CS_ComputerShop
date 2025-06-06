using Firma.Data.Data.Shop; 
using Firma.PortalWWW.Models.Shop;

namespace Firma.PortalWWW.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(int productId, int quantity = 1);
        Task<List<CartItemViewModel>> GetCartItemsAsync(); 
        Task RemoveFromCartAsync(int productId);
        Task UpdateQuantityAsync(int productId, int quantity);
        Task<int> GetCartItemCountAsync();
        Task<decimal> GetCartTotalAsync();
        Task ClearCartAsync();
        Task MigrateCartAsync(int userId); // Do migracji koszyka po zalogowaniu
    }
}