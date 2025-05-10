using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Firma.Data.Data.Customers;


namespace Firma.Data.Data.Shop
{
    public class Cart
    {
        [Key]
        public int IdCart { get; set; }

        [Required(ErrorMessage = "IdUser is required")]
        public int IdUser { get; set; }

        [ForeignKey("IdUser")]
        public User? User { get; set; }

        [Required(ErrorMessage = "IdProduct is required")]
        public int IdProduct { get; set; }

        [ForeignKey("IdProduct")]
        public Product? Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
