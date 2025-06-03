using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Firma.Data.Data.CMS
{

    public enum PageMenuPlacement
    {
        [Display(Name = "Główne menu")] // Atrybut dla lepszego wyświetlania w Intranecie
        MainMenu,

        [Display(Name = "Menu w stopce")]
        FooterMenu,

        [Display(Name = "Nigdzie (tylko bezpośredni link)")]
        None
    }

    public class Page
    {
        [Key] //to co niżej będzie kluczem podstawowym tabeli
        public int IdPage { get; set; }

        [Required(ErrorMessage ="Title link is required")]//to co niżej jest wymagane
        [MaxLength(255, ErrorMessage ="Link can contain max 50 characters")]//maksymalny rozmiar
        [Display(Name ="Title link")] //tak ma nazywać się pole widoczne na widoku
        public required string TitleLink { get; set; }

        [Required(ErrorMessage ="Page title is required")]
        [MaxLength(255, ErrorMessage ="Page title can contain max 50 characters")]
        [Display(Name = "Page title")]
        public required string Title { get; set; }

        [Display(Name = "Page content")]
        [Column(TypeName ="nvarchar(MAX)")] //decyzja o typie pola w bazie danych
        public required string Contents { get; set; }

        [Required(ErrorMessage = "Page position is required")]
        [Display(Name = "Page position")]
        public int Position { get; set; }

        [Display(Name = "Miejsce wyświetlania w menu")]
        public PageMenuPlacement MenuPlacement { get; set; } = PageMenuPlacement.None; // Domyślnie strona nie jest w żadnym menu
    }
}
