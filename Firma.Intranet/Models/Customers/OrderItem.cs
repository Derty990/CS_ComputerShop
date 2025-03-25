using Firma.Intranet.Models.Shop;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Firma.Intranet.Models.Customers
{
    public class OrderItem
    {
        [Key]
        public int IdOrderItem { get; set; }

        [Required]
        public int IdOrder { get; set; }

        [ForeignKey("IdOrder")]
        public Order? Order { get; set; }

        [Required]
        public int IdProduct { get; set; }

        [ForeignKey("IdProduct")]
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "money")]
        public decimal UnitPrice { get; set; }
    }
}
