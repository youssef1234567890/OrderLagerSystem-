namespace OrderLagerSystem.Data
{
    public class OrderHistory
    {
        public int OrderHistoryId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty;

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}