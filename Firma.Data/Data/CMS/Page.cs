using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firma.Data.Data.CMS
{
    public class Page
    {
        [Key] //to co niżej będzie kluczem podstawowym tabeli
        public int IdPage { get; set; }

        [Required(ErrorMessage ="Title link is required")]//to co niżej jest wymagane
        [MaxLength(50, ErrorMessage ="Link can contain max 50 characters")]//maksymalny rozmiar
        [Display(Name ="Title link")] //tak ma nazywać się pole widoczne na widoku
        public required string TitleLink { get; set; }

        [Required(ErrorMessage ="Page title is required")]
        [MaxLength(50, ErrorMessage ="Page title can contain max 50 characters")]
        [Display(Name = "Page title")]
        public required string Title { get; set; }

        [Display(Name = "Page content")]
        [Column(TypeName ="nvarchar(MAX)")] //decyzja o typie pola w bazie danych
        public required string Contents { get; set; }

        [Required(ErrorMessage = "Page position is required")]
        [Display(Name = "Page position")]
        public int Position { get; set; }
    }
}
