using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Firma.PortalWWW.Models.Shop
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal CartTotal { get; set; }

        [Required(ErrorMessage = "Imię jest wymagane")]
        [Display(Name = "Imię")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Adres jest wymagany")]
        [Display(Name = "Adres (ulica i numer)")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane")]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany")]
        [Display(Name = "Kod pocztowy")]
        public string PostalCode { get; set; }

        [Display(Name = "Uwagi do zamówienia (opcjonalnie)")]
        public string? Notes { get; set; }
    }
}