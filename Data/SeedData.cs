using Microsoft.EntityFrameworkCore;
using TiendaOnline.Models;
namespace TiendaOnline.Data;
public static class SeedData {
 public static async Task LoadAsync(ShopDbContext db) {
  if (await db.Products.AnyAsync()) return;
  db.Products.AddRange(
   new Product{Name="Laptop Pro 14",Category="Tecnología",Description="Portátil para trabajo y estudio",Price=3799,Stock=12,Emoji="💻"},
   new Product{Name="Audífonos inalámbricos",Category="Accesorios",Description="Sonido nítido y batería duradera",Price=279,Stock=30,Emoji="🎧"},
   new Product{Name="Smartwatch Active",Category="Tecnología",Description="Reloj inteligente para actividades diarias",Price=599,Stock=20,Emoji="⌚"},
   new Product{Name="Mochila urbana",Category="Accesorios",Description="Compartimento para laptop y diseño resistente",Price=189,Stock=35,Emoji="🎒"},
   new Product{Name="Teclado mecánico",Category="Tecnología",Description="Teclado compacto y cómodo",Price=349,Stock=18,Emoji="⌨️"},
   new Product{Name="Cámara digital",Category="Fotografía",Description="Captura tus mejores momentos",Price=1299,Stock=8,Emoji="📷"}
  );
  await db.SaveChangesAsync();
 }
}
