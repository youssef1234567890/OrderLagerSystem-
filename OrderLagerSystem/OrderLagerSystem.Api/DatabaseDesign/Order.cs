namespace OrderLagerSystem.Data
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public Delivery? Delivery { get; set; }
        public ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();
    }
}