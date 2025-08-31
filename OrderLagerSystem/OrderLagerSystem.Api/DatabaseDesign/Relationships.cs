using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OrderLagerSystem.Api.DatabaseDesign
{
    public static class Relationships
    {
        // Anropas från DbContext.OnModelCreating(modelBuilder)
        public static void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleConfig());
        }

        private sealed class ArticleConfig : IEntityTypeConfiguration<Article>
        {
            public void Configure(EntityTypeBuilder<Article> b)
            {
                b.ToTable("Articles");
                b.HasKey(x => x.ArticleId);

                b.Property(x => x.Sku).IsRequired().HasMaxLength(GlobalRules.SkuMaxLen);
                b.Property(x => x.Name).IsRequired().HasMaxLength(GlobalRules.NameMaxLen);
                b.Property(x => x.Description).HasMaxLength(GlobalRules.DescriptionMaxLen);

                // SQLite-typer
                b.Property(x => x.PriceInCents).HasColumnType("INTEGER");
                b.Property(x => x.StockQuantity).HasColumnType("INTEGER");
                b.Property(x => x.CreatedUtc).HasColumnType("TEXT");
                b.Property(x => x.UpdatedUtc).HasColumnType("TEXT");

                // Index
                b.HasIndex(x => x.Sku).IsUnique(); // SKU ska vara unik
                b.HasIndex(x => x.Name);           // Sökning/sortering

                // Check-constraints (stöd i SQLite)
                b.HasCheckConstraint("CK_Articles_Price_NonNegative", "PriceInCents >= 0");
                b.HasCheckConstraint("CK_Articles_Stock_NonNegative", "StockQuantity >= 0");
                b.HasCheckConstraint("CK_Articles_Sku_NotEmpty", "length(Sku) > 0");

                // Relation: Article (1) → OrderItems (N)
                // Skugg-FK "ArticleId" så moment 4 kan lägga till riktig FK utan konflikt
                b.HasMany(a => a.OrderItems)
                 .WithOne() // ingen nav på OrderItem här
                 .HasForeignKey("ArticleId")
                 .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
}
