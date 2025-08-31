using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderLagerSystem.Api.DatabaseDesign;

public static class Relationships
{
    /// <summary>
    /// Anropas från din DbContext.OnModelCreating(modelBuilder).
    /// </summary>
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UsersConfig());
        modelBuilder.ApplyConfiguration(new ArticleConfig());
        modelBuilder.ApplyConfiguration(new OrderConfig());
        modelBuilder.ApplyConfiguration(new OrderItemConfig());
        modelBuilder.ApplyConfiguration(new DeliveryConfig());
        modelBuilder.ApplyConfiguration(new OrderHistoryConfig());
    }

    // ========================= Users =========================
    private sealed class UsersConfig : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> b)
        {
            b.ToTable("Users");

            b.HasKey(x => x.UsersId);

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(GlobalRules.NameMaxLen);

            b.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(GlobalRules.EmailMaxLen);

            b.Property(x => x.CreatedUtc)
                .HasColumnType("TEXT");

            // En användare per e-postadress
            b.HasIndex(x => x.Email).IsUnique();
        }
    }

    // ========================= Articles =========================
    private sealed class ArticleConfig : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> b)
        {
            // Check-constraints via TableBuilder (nytt sätt i EF Core)
            b.ToTable("Articles", tb =>
            {
                tb.HasCheckConstraint("CK_Articles_Price_NonNegative", "PriceInCents >= 0");
                tb.HasCheckConstraint("CK_Articles_Stock_NonNegative", "StockQuantity >= 0");
                tb.HasCheckConstraint("CK_Articles_Sku_NotEmpty", "length(Sku) > 0");
            });

            b.HasKey(x => x.ArticleId);

            b.Property(x => x.Sku)
                .IsRequired()
                .HasMaxLength(GlobalRules.SkuMaxLen);

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(GlobalRules.NameMaxLen);

            b.Property(x => x.Description)
                .HasMaxLength(GlobalRules.DescriptionMaxLen);

            // Typmappning för SQLite (frivilligt men tydligt)
            b.Property(x => x.PriceInCents).HasColumnType("INTEGER");
            b.Property(x => x.StockQuantity).HasColumnType("INTEGER");
            b.Property(x => x.CreatedUtc).HasColumnType("TEXT");
            b.Property(x => x.UpdatedUtc).HasColumnType("TEXT");

            // Index
            b.HasIndex(x => x.Sku).IsUnique();
            b.HasIndex(x => x.Name);

            // Relation: Article (1) → OrderItems (N)
            // Förhindrar radering av artikel som används i orderrader
            b.HasMany(a => a.OrderItems)
             .WithOne(oi => oi.Article)
             .HasForeignKey(oi => oi.ArticleId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // ========================= Orders =========================
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

            b.HasIndex(x => x.UsersId);
            b.HasIndex(x => x.ExternalOrderNo);

            // User (1) → Orders (N). Restrict så ordrar inte raderas av misstag.
            b.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UsersId)
             .OnDelete(DeleteBehavior.Restrict);

            // Order (1) → OrderItems (N). Radera rader när order tas bort.
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

    // ========================= OrderItems =========================
    private sealed class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> b)
        {
            // Check-constraints via TableBuilder
            b.ToTable("OrderItems", tb =>
            {
                tb.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "Quantity > 0");
                tb.HasCheckConstraint("CK_OrderItems_UnitPrice_NonNegative", "UnitPriceInCents >= 0");
            });

            b.HasKey(x => x.OrderItemId);

            b.Property(x => x.Quantity).HasColumnType("INTEGER");
            b.Property(x => x.UnitPriceInCents).HasColumnType("INTEGER");

            // En artikel får inte förekomma två gånger i samma order
            b.HasIndex(x => new { x.OrderId, x.ArticleId })
             .IsUnique()
             .HasDatabaseName("UX_OrderItem_Order_Article");

            // FK-konfigurationerna till Orders/Articles sätts i respektive configs.
        }
    }

    // ========================= Deliveries =========================
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
            b.HasIndex(x => x.TrackingNumber).IsUnique(); // SQLite tillåter flera NULL
        }
    }

    // ========================= OrderHistory =========================
    private sealed class OrderHistoryConfig : IEntityTypeConfiguration<OrderHistory>
    {
        public void Configure(EntityTypeBuilder<OrderHistory> b)
        {
            b.ToTable("OrderHistory");

            b.HasKey(x => x.OrderHistoryId);

            b.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(GlobalRules.StatusMaxLen);

            b.Property(x => x.Comment)
                .HasMaxLength(GlobalRules.DescriptionMaxLen);

            b.Property(x => x.ChangedUtc).HasColumnType("TEXT");

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.ChangedUtc);
        }
    }
}
