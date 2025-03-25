using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firma.Intranet.Models.Shop
{
    public class ProductType
    {
        [Key]
        public int IdProductType { get; set; }

        [Required(ErrorMessage ="Product type name is required")]
        [MaxLength(30, ErrorMessage ="Type of product can containt max 30 characters")]
        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        //to jest obsługa relacji 1 do wielu (towar ma jeden rodzaj, rodzaj ma wiele towarów danego rodzaju)
        //kod po stronie wiele - rodzaj ma wiele towarów danego rodzaju
        public ICollection<Product> Products { get; } = new List<Product>();

    }
}
