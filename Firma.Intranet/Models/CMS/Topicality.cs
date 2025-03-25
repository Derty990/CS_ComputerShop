using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Firma.Intranet.Models.CMS
{
    public class Topicality
    {
        [Key] //to co niżej będzie kluczem podstawowym tabeli
        public int IdTopicality { get; set; }

        [Required(ErrorMessage = "Title link is required")]//to co niżej jest wymagane
        [MaxLength(50, ErrorMessage = "Link can contain max 50 characters")]//maksymalny rozmiar
        [Display(Name = "Title link")] //tak ma nazywać się pole widoczne na widoku
        public required string TitleLink { get; set; }

        [Required(ErrorMessage = "Topicality title is required")]
        [MaxLength(50, ErrorMessage = "Topicality title can contain max 50 characters")]
        [Display(Name = "Topicality title")]
        public required string Title { get; set; }

        [Display(Name = "Topicality content")]
        [Column(TypeName = "nvarchar(MAX)")] //decyzja o typie pola w bazie danych
        public required string Contents { get; set; }

        [Required(ErrorMessage = "Topicality position is required")]
        [Display(Name = "Topicality position")]
        public int Position { get; set; }
    }
}
