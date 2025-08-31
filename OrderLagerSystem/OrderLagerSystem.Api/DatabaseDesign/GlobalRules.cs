using System;

namespace OrderLagerSystem.Api.DatabaseDesign;

public static class GlobalRules
{
    // Gemensamma maxlängder
    public const int SkuMaxLen = 64;
    public const int NameMaxLen = 200;
    public const int DescriptionMaxLen = 2000;
    public const int EmailMaxLen = 320;
    public const int StatusMaxLen = 64;
    public const int TrackingMaxLen = 80;

    // Pengar sparas som öre (INTEGER) för att undvika decimalstrul i SQLite
    public const string MoneyNote = "Money stored as cents (INTEGER) for SQLite.";
}
