using System.ComponentModel.DataAnnotations;
namespace TiendaOnline.Models;
public class Order {
 public int Id { get; set; }
 [Required, MaxLength(120)] public string CustomerName { get; set; } = "";
 [Required, EmailAddress, MaxLength(200)] public string Email { get; set; } = "";
 [Required, MaxLength(300)] public string Address { get; set; } = "";
 public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
 public decimal Total { get; set; }
 public string Status { get; set; } = "PENDIENTE_PAGO";
 public List<OrderItem> Items { get; set; } = new();
}
public class OrderItem {
 public int Id { get; set; }
 public int OrderId { get; set; }
 public Order Order { get; set; } = null!;
 public int ProductId { get; set; }
 public string ProductName { get; set; } = "";
 public int Quantity { get; set; }
 public decimal UnitPrice { get; set; }
}
