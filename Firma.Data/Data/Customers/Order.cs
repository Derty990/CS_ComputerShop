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
