using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Firma.Intranet.Models.Customers;


namespace Firma.Intranet.Models.Shop
{
    public class Cart
    {
        [Key]
        public int IdCart { get; set; }

        [Required]
        public int IdUser { get; set; }

        [ForeignKey("IdUser")]
        public required User User { get; set; }

        [Required]
        public int IdProduct { get; set; }

        [ForeignKey("IdProduct")]
        public required Product Product { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
