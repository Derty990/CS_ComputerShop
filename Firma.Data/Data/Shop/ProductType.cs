using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firma.Data.Data.Shop
{
    public class ProductType
    {
        [Key]
        public int IdProductType { get; set; }

        [Required(ErrorMessage ="Product type name is required")]
        [MaxLength(30, ErrorMessage ="Type of product can containt max 30 characters")]
        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        [Display(Name = "URL zdjęcia")]
        [MaxLength(255)] // Przykładowa maksymalna długość URL
        public string? PhotoUrl { get; set; } // Może być null, jeśli kategoria nie ma zdjęcia

        [Display(Name = "Polecana na stronie głównej?")]
        public bool IsFeaturedOnHomepage { get; set; } = false;

        [Display(Name = "Kolejność wyświetlania (SG)")]
        public int? DisplayOrderHomepage { get; set; } // Nullable, jeśli nie wszystkie polecane muszą mieć kolejność

        //to jest obsługa relacji 1 do wielu (towar ma jeden rodzaj, rodzaj ma wiele towarów danego rodzaju)
        //kod po stronie wiele - rodzaj ma wiele towarów danego rodzaju
        public ICollection<Product> Products { get; } = new List<Product>();

    }
}
