using System.ComponentModel.DataAnnotations;
namespace TiendaOnline.Models;
public class Product {
 public int Id { get; set; }
 [Required, MaxLength(120)] public string Name { get; set; } = "";
 [MaxLength(700)] public string Description { get; set; } = "";
 [Range(0, 999999)] public decimal Price { get; set; }
 [Range(0, 100000)] public int Stock { get; set; }
 [MaxLength(100)] public string Category { get; set; } = "General";
 public string Emoji { get; set; } = "🛍️";
}
public record RelatedProduct(Product Product, double Similarity);
