using Microsoft.EntityFrameworkCore;
using TiendaOnline.Models;
namespace TiendaOnline.Data;
public class ShopDbContext(DbContextOptions<ShopDbContext> options) : DbContext(options) {
 public DbSet<Product> Products => Set<Product>();
 public DbSet<Order> Orders => Set<Order>();
 public DbSet<OrderItem> OrderItems => Set<OrderItem>();
 public DbSet<Promotion> Promotions => Set<Promotion>();
 public DbSet<BusinessEvent> BusinessEvents => Set<BusinessEvent>();
 public DbSet<SupplierImport> SupplierImports => Set<SupplierImport>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<Product>().Property(x=>x.Price).HasPrecision(12,2);
  b.Entity<Order>().Property(x=>x.Total).HasPrecision(12,2);
  b.Entity<OrderItem>().Property(x=>x.UnitPrice).HasPrecision(12,2);
  b.Entity<Promotion>().Property(x=>x.DiscountPercent).HasPrecision(5,2);
  b.Entity<BusinessEvent>().Property(x=>x.Amount).HasPrecision(12,2);
  b.Entity<Order>().HasMany(x=>x.Items).WithOne(x=>x.Order).HasForeignKey(x=>x.OrderId);
  b.Entity<Promotion>().HasIndex(x=>x.Code).IsUnique();
  b.Entity<BusinessEvent>().HasIndex(x=>new{x.CreatedUtc,x.EventType});
 }
}
