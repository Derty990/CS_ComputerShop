using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Firma.Data.Data.Customers
{
    public class Order
    {
        [Key]
        public int IdOrder { get; set; }
        
        [Required(ErrorMessage ="Error in IdUser field, Order table")]
        public int IdUser { get; set; }

        [ForeignKey("IdUser")]
        public User? User { get; set; }

        [Required(ErrorMessage = "Error in OrderDat field, Order table")]
        [Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Error in TypeName field, Order table")]
        [Column(TypeName = "nvarchar(20)")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "money")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [MaxLength(100)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        [MaxLength(50)]
        public string City { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [MaxLength(10)]
        public string PostalCode { get; set; }

        public string? Notes { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
       
    }

    public enum OrderStatus
    {
        Pending,
        Shipped,
        Delivered,
        Cancelled
    }


}
