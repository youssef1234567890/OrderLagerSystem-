using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderLagerSystem.Models;

namespace OrderLagerSystem.Data
{
    /// <summary>
    /// Huvud-DbContext för Order- och Lagersystemet
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets för alla tabeller
        public DbSet<Article> Articles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<Role> SystemRoles { get; set; }
        public DbSet<UserRole> SystemUserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Article konfiguration
            builder.Entity<Article>(entity =>
            {
                entity.HasKey(a => a.ArticleId);
                entity.HasIndex(a => a.Sku).IsUnique();
                entity.Property(a => a.Sku).IsRequired();
                entity.Property(a => a.Name).IsRequired();
            });

            // Order konfiguration
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.HasOne(o => o.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderItem konfiguration
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(oi => oi.Article)
                    .WithMany(a => a.OrderItems)
                    .HasForeignKey(oi => oi.ArticleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // OrderHistory konfiguration
            builder.Entity<OrderHistory>(entity =>
            {
                entity.HasKey(oh => oh.OrderHistoryId);
                entity.HasOne(oh => oh.Order)
                    .WithMany(o => o.History)
                    .HasForeignKey(oh => oh.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(oh => oh.ChangedByUser)
                    .WithMany(u => u.OrderHistoryEntries)
                    .HasForeignKey(oh => oh.ChangedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Delivery konfiguration
            builder.Entity<Delivery>(entity =>
            {
                entity.HasKey(d => d.DeliveryId);
                entity.HasOne(d => d.Order)
                    .WithMany(o => o.Deliveries)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // StockMovement konfiguration
            builder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(sm => sm.StockMovementId);
                entity.HasOne(sm => sm.Article)
                    .WithMany(a => a.StockMovements)
                    .HasForeignKey(sm => sm.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(sm => sm.User)
                    .WithMany(u => u.StockMovements)
                    .HasForeignKey(sm => sm.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(sm => sm.Order)
                    .WithMany()
                    .HasForeignKey(sm => sm.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Role konfiguration
            builder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);
                entity.HasIndex(r => r.Name).IsUnique();
                entity.Property(r => r.Name).IsRequired();
            });

            // UserRole konfiguration (many-to-many)
            builder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(ur => ur.AssignedByUser)
                    .WithMany(u => u.AssignedRoles)
                    .HasForeignKey(ur => ur.AssignedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Seed data för roller
            builder.Entity<Role>().HasData(
                new Role { RoleId = 1, Name = GlobalRules.Roles.Admin, Description = "Systemadministratör med full åtkomst", CreatedUtc = DateTime.UtcNow },
                new Role { RoleId = 2, Name = GlobalRules.Roles.Orderkoordinator, Description = "Hanterar order och leveranser", CreatedUtc = DateTime.UtcNow },
                new Role { RoleId = 3, Name = GlobalRules.Roles.Employee, Description = "Hanterar lager och inleveranser", CreatedUtc = DateTime.UtcNow }
            );
        }
    }
}