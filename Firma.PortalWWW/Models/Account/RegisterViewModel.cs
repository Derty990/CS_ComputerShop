using System.ComponentModel.DataAnnotations;

namespace Firma.PortalWWW.Models.Account
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Imię i nazwisko / Nazwa jest wymagane.")]
        [MaxLength(50, ErrorMessage = "Nazwa może zawierać max 50 znaków.")]
        [Display(Name = "Imię i nazwisko / Nazwa")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Adres email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawidłowy format adresu email.")]
        [Display(Name = "Adres email")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Numer telefonu jest wymagany.")]
        [Phone(ErrorMessage = "Nieprawidłowy format numeru telefonu.")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi składać się dokładnie z 9 cyfr.")]
        [Display(Name = "Numer telefonu")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Hasło jest wymagane.")]
        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        // Wyrażenie regularne sprawdzające: min. 1 wielka litera, min. 1 cyfra, min. 1 znak specjalny.
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
                         ErrorMessage = "Hasło musi mieć co najmniej 6 znaków, zawierać minimum jedną wielką literę, jedną cyfrę oraz jeden znak specjalny (np. @, $, !, %, *, ?, &).")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Hasło i jego potwierdzenie nie są zgodne.")]
        public required string ConfirmPassword { get; set; }
    }
}