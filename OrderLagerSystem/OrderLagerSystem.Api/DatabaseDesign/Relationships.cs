using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderLagerSystem.Api.DatabaseDesign;

public static class Relationships
{
    // Anropas från DbContext.OnModelCreating(modelBuilder)
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersConfig());
        modelBuilder.ApplyConfiguration(new ArticleConfig());
        modelBuilder.ApplyConfiguration(new OrderConfig());
        modelBuilder.ApplyConfiguration(new OrderItemConfig());
        modelBuilder.ApplyConfiguration(new DeliveryConfig());
        modelBuilder.ApplyConfiguration(new OrderHistoryConfig());
    }

    // ---- Users
    private sealed class UsersConfig : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> b)
        {
            b.ToTable("Users");
            b.HasKey(x => x.UsersId);

            b.Property(x => x.Name).IsRequired().HasMaxLength(GlobalRules.NameMaxLen);
            b.Property(x => x.Email).IsRequired().HasMaxLength(GlobalRules.EmailMaxLen);
            b.Property(x => x.CreatedUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.Email).IsUnique(); // en användare per e-post
        }
    }

    // ---- Articles
    private sealed class ArticleConfig : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> b)
        {
            b.ToTable("Articles");
            b.HasKey(x => x.ArticleId);

            b.Property(x => x.Sku).IsRequired().HasMaxLength(GlobalRules.SkuMaxLen);
            b.Property(x => x.Name).IsRequired().HasMaxLength(GlobalRules.NameMaxLen);
            b.Property(x => x.Description).HasMaxLength(GlobalRules.DescriptionMaxLen);

            b.Property(x => x.PriceInCents).HasColumnType("INTEGER");
            b.Property(x => x.StockQuantity).HasColumnType("INTEGER");
            b.Property(x => x.CreatedUtc).HasColumnType("TEXT");
            b.Property(x => x.UpdatedUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.Sku).IsUnique();
            b.HasIndex(x => x.Name);

            b.HasCheckConstraint("CK_Articles_Price_NonNegative", "PriceInCents >= 0");
            b.HasCheckConstraint("CK_Articles_Stock_NonNegative", "StockQuantity >= 0");

            // Relation: Article (1) → OrderItems (N), radering av artikel tillåts inte om orderrader finns
            b.HasMany(a => a.OrderItems)
             .WithOne(oi => oi.Article)
             .HasForeignKey(oi => oi.ArticleId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ---- Orders
    private sealed class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> b)
        {
            b.ToTable("Orders");
            b.HasKey(x => x.OrderId);

            b.Property(x => x.ExternalOrderNo).HasMaxLength(GlobalRules.NameMaxLen);
            b.Property(x => x.Status).HasMaxLength(GlobalRules.StatusMaxLen);
            b.Property(x => x.CreatedUtc).HasColumnType("TEXT");
            b.Property(x => x.PaidUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.ExternalOrderNo);
            b.HasIndex(x => x.UsersId);

            // User (1) → Orders (N). Om en user tas bort: behåll Orders (Restrict).
            b.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UsersId)
             .OnDelete(DeleteBehavior.Restrict);

            // Order (1) → OrderItems (N). Om en order tas bort: ta även bort raderna (Cascade).
            b.HasMany(o => o.Items)
             .WithOne(oi => oi.Order)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // Order (1) → Deliveries (N)
            b.HasMany(o => o.Deliveries)
             .WithOne(d => d.Order)
             .HasForeignKey(d => d.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            // Order (1) → OrderHistory (N)
            b.HasMany(o => o.History)
             .WithOne(h => h.Order)
             .HasForeignKey(h => h.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }

    // ---- OrderItems
    private sealed class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            b.ToTable("OrderItems");
            b.HasKey(x => x.OrderItemId);

            b.Property(x => x.Quantity).HasColumnType("INTEGER");
            b.Property(x => x.UnitPriceInCents).HasColumnType("INTEGER");

            // 1 artikel får inte förekomma två gånger i samma order
            b.HasIndex(x => new { x.OrderId, x.ArticleId })
             .IsUnique()
             .HasDatabaseName("UX_OrderItem_Order_Article");

            b.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "Quantity > 0");
            b.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "UnitPriceInCents >= 0");

            // FK-konfigurationer görs i ArticleConfig/OrderConfig (ovan)
        }
    }

    // ---- Deliveries
    private sealed class DeliveryConfig : IEntityTypeConfiguration<Delivery>
    {
        public void Configure(EntityTypeBuilder<Delivery> b)
        {
            b.ToTable("Deliveries");
            b.HasKey(x => x.DeliveryId);

            b.Property(x => x.Status).HasMaxLength(GlobalRules.StatusMaxLen);
            b.Property(x => x.TrackingNumber).HasMaxLength(GlobalRules.TrackingMaxLen);
            b.Property(x => x.CreatedUtc).HasColumnType("TEXT");
            b.Property(x => x.ShippedUtc).HasColumnType("TEXT");
            b.Property(x => x.DeliveredUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.TrackingNumber).IsUnique(); // n/a om null i SQLite – OK
        }
    }

    // ---- OrderHistory
    private sealed class OrderHistoryConfig : IEntityTypeConfiguration<OrderHistory>
    {
        public void Configure(EntityTypeBuilder<OrderHistory> b)
        {
            b.ToTable("OrderHistory");
            b.HasKey(x => x.OrderHistoryId);

            b.Property(x => x.Status).IsRequired().HasMaxLength(GlobalRules.StatusMaxLen);
            b.Property(x => x.Comment).HasMaxLength(GlobalRules.DescriptionMaxLen);
            b.Property(x => x.ChangedUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.ChangedUtc);
        }
    }
}
