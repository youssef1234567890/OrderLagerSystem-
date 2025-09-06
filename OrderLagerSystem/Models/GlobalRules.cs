namespace OrderLagerSystem.Models;

/// <summary>
/// Globala regler och konstanter för hela systemet
/// </summary>
public static class GlobalRules
{
    // Gemensamma maxlängder för datavalidering
    public const int SkuMaxLen = 64;
    public const int NameMaxLen = 200;
    public const int DescriptionMaxLen = 2000;
    public const int EmailMaxLen = 320;
    public const int StatusMaxLen = 64;
    public const int TrackingMaxLen = 80;
    public const int LocationMaxLen = 100;
    public const int CommentMaxLen = 1000;

    // Pengar sparas som öre (INTEGER) för att undvika decimalstrul i SQLite
    public const string MoneyNote = "Money stored as cents (INTEGER) for SQLite.";

    // Order statusar
    public static class OrderStatus
    {
        public const string Created = "Created";
        public const string Confirmed = "Confirmed";
        public const string Processing = "Processing";
        public const string Shipped = "Shipped";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
    }

    // Delivery statusar
    public static class DeliveryStatus
    {
        public const string Pending = "Pending";
        public const string Preparing = "Preparing";
        public const string Shipped = "Shipped";
        public const string InTransit = "InTransit";
        public const string Delivered = "Delivered";
        public const string Failed = "Failed";
    }

    // Användarroller
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Orderkoordinator = "Orderkoordinator";
        public const string Employee = "Employee";
    }
}
