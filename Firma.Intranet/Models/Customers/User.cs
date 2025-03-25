using System.ComponentModel.DataAnnotations;
using Firma.Intranet.Models.Shop;
using Firma.Intranet.Models.Customers;

namespace Firma.Intranet.Models.Customers
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Wrong email format")]
        [MaxLength(100)]
        public required string Email { get; set; }

        [Required(ErrorMessage ="HashedPassword is required")]
        [MaxLength(256)]
        public required string HashedPassword { get; set; } 

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(50)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [MaxLength(15)]
        [Phone(ErrorMessage = "Wrong phone number format, area code is required")]
        public required string Phone { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Cart> CartItems { get; set; } = new List<Cart>();

       
    }

    public enum UserRole
    {
        User,
        Admin,
        Moderator
    }


}
