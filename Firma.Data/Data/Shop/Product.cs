using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Firma.Data.Data.Shop
{
    public class Product
    {
        [Key]
        public int IdProduct { get; set; }

        [Required(ErrorMessage = "Product code is required")]
        [MaxLength(20, ErrorMessage = "Code max length is 20")]
        public required string Code { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Product price is required")]
        [Column(TypeName = "money")]
        public decimal Price { get; set; }

        [Display(Name = "Dostępność (w magazynie)")]
        public bool IsInStock { get; set; } = true; // Domyślnie produkt jest dostępny

        [Required(ErrorMessage = "Product photo is required")]
        [Display(Name = "Product photo")]
        public required string PhotoUrl { get; set; }

        public string Description { get; set; } = string.Empty;

        //to jest obsługa relacji 1 do wielu (towar ma jeden rodzaj, rodzaj ma wiele towarów danego rodzaju)
        //kod po stronie jeden - towar ma jeden rodzaj
        [ForeignKey("ProductType")]
        public int IdProductType { get; set; }

        public ProductType? ProductType { get; set; }

        [Display(Name = "Polecany na stronie głównej?")]
        public bool IsFeaturedOnHomepage { get; set; } = false;

        [Display(Name = "Kolejność wyświetlania (SG)")]
        public int? DisplayOrderHomepage { get; set; }


    }
}
