using System.ComponentModel.DataAnnotations;

namespace Firma.PortalWWW.Models.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imię i nazwisko jest wymagane")]
        [Display(Name = "Imię i nazwisko")]
        [StringLength(50, ErrorMessage = "Imię i nazwisko nie może przekraczać 50 znaków")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Adres email jest wymagany")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email")]
        [Display(Name = "Email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu")]
        [Display(Name = "Telefon")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane")]
        [StringLength(100, ErrorMessage = "Hasło musi mieć co najmniej {2} znaków długości.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne.")]
        public required string ConfirmPassword { get; set; }
    }
}
