using EstoqueFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoqueFlow.Web.Data;

public class EstoqueFlowContext(DbContextOptions<EstoqueFlowContext> options)
    : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(10, 2);
        modelBuilder.Entity<Product>().HasIndex(p => p.Name);
        modelBuilder.Entity<PurchaseOrder>().Property(p => p.UnitPrice).HasPrecision(10, 2);
        modelBuilder.Entity<SalesOrder>().Property(s => s.UnitPrice).HasPrecision(10, 2);

        modelBuilder.Entity<SalesOrder>()
            .HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SalesOrder>()
            .HasOne(s => s.Product).WithMany().HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.Supplier).WithMany().HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(p => p.Product).WithMany().HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<StockMovement>()
            .HasOne(m => m.Product).WithMany().HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique();
        modelBuilder.Entity<SystemSettings>()
            .HasKey(s => s.Id);
    }
}
