namespace OrderLagerSystem.Data
{
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = "Pending";

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}