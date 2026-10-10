namespace TiendaOnline.Models;

// Datos de la portada y de la página de resultados (filtros estilo marketplace).
public class StorePageViewModel {
 public string? Query { get; set; }
 public string? Category { get; set; }
 public string Sort { get; set; } = "relevancia";
 public decimal? Min { get; set; }
 public decimal? Max { get; set; }
 public bool InStockOnly { get; set; }
 public bool IsSearch => !string.IsNullOrWhiteSpace(Query) || !string.IsNullOrWhiteSpace(Category) || Min.HasValue || Max.HasValue || InStockOnly;

 public List<Product> Results { get; set; } = new();
 public List<RelatedProduct> Related { get; set; } = new();
 public string? SemanticUnavailable { get; set; }
 // Conteo por categoría sobre la búsqueda por texto (sin aplicar el filtro de categoría).
 public List<(string Category, int Count)> CategoryCounts { get; set; } = new();
 // Portada: productos agrupados por categoría.
 public List<(string Category, List<Product> Products)> CategoryTiles { get; set; } = new();

 public static readonly (string Label, decimal? Min, decimal? Max)[] PriceRanges = {
  ("Hasta S/ 300", null, 300m), ("S/ 300 a S/ 800", 300m, 800m), ("S/ 800 a S/ 1,500", 800m, 1500m), ("Más de S/ 1,500", 1500m, null)
 };
 public static readonly (string Value, string Label)[] SortOptions = {
  ("relevancia", "Destacados"), ("precio-asc", "Precio: de menor a mayor"), ("precio-desc", "Precio: de mayor a menor"), ("nombre", "Nombre: A-Z")
 };
}
