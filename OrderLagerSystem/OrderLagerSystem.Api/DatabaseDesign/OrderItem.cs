namespace OrderLagerSystem.Api.DatabaseDesign;

public class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public int Quantity { get; set; }
    public long UnitPriceInCents { get; set; } // pris vid ordertillfället
}