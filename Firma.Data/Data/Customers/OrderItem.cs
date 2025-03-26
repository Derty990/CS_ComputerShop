using Firma.Data.Data.Shop;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Firma.Data.Data.Customers
{
    public class OrderItem
    {
        [Key]
        public int IdOrderItem { get; set; }

        [Required(ErrorMessage = "IdOrder is required")]
        public int IdOrder { get; set; }

        [ForeignKey("IdOrder")]
        public Order? Order { get; set; }

        [Required(ErrorMessage = "IdProduct is required")]
        public int IdProduct { get; set; }

        [ForeignKey("IdProduct")]
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "UnitPrice is required")]
        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }
    }
}
