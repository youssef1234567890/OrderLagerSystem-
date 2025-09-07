using Microsoft.AspNetCore.Identity;
using OrderLagerSystem.Models;

namespace OrderLagerSystem.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Seed roller
            var roles = new[] { GlobalRules.Roles.Admin, GlobalRules.Roles.Orderkoordinator, GlobalRules.Roles.Employee };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed användare med realistiska roller
            var admin = new ApplicationUser 
            { 
                UserName = "shalan.mourad@datorlager.se", 
                Email = "shalan.mourad@datorlager.se", 
                EmailConfirmed = true, 
                FullName = "Shahlan Mourad", 
                FirstName = "Shahlan", 
                LastName = "Mourad", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };
            var orderCoordinator = new ApplicationUser 
            { 
                UserName = "Adel.Ali@datorlager.se", 
                Email = "Adel.Ali@datorlager.se", 
                EmailConfirmed = true, 
                FullName = "Adel Ali", 
                FirstName = "Adel", 
                LastName = "Ali", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };
            var employee = new ApplicationUser 
            { 
                UserName = "log.don@datorlager.se", 
                Email = "log.don@datorlager.se", 
                EmailConfirmed = true, 
                FullName = "Log Don", 
                FirstName = "Log", 
                LastName = "Don", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, GlobalRules.Roles.Admin);
            }

            if (await userManager.FindByEmailAsync(orderCoordinator.Email) == null)
            {
                await userManager.CreateAsync(orderCoordinator, "Order123!");
                await userManager.AddToRoleAsync(orderCoordinator, GlobalRules.Roles.Orderkoordinator);
            }

            if (await userManager.FindByEmailAsync(employee.Email) == null)
            {
                await userManager.CreateAsync(employee, "Employee123!");
                await userManager.AddToRoleAsync(employee, GlobalRules.Roles.Employee);
            }

            // 3. Seed datorprodukter
            if (!context.Articles.Any())
            {
                context.Articles.AddRange(
                    new Article 
                    { 
                        Sku = "LAPTOP-DELL-001", 
                        Name = "Dell XPS 13 Laptop", 
                        PriceInCents = 1299500, // 12,995 kr
                        StockQuantity = 15, 
                        MinimumStock = 5,
                        StorageLocation = "A1-B2",
                        Description = "13-tums ultrabook med Intel Core i7, 16GB RAM, 512GB SSD. Perfekt för professionellt arbete och studentanvändning.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "DESKTOP-HP-002", 
                        Name = "HP Pavilion Desktop", 
                        PriceInCents = 849900, // 8,499 kr
                        StockQuantity = 8, 
                        MinimumStock = 3,
                        StorageLocation = "B2-C1",
                        Description = "Kraftfull desktop-dator med AMD Ryzen 5, 8GB RAM, 1TB HDD + 256GB SSD. Idealisk för hemmakontor och gaming.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "MONITOR-SAMSUNG-003", 
                        Name = "Samsung 27\" 4K Monitor", 
                        PriceInCents = 349900, // 3,499 kr
                        StockQuantity = 25, 
                        MinimumStock = 10,
                        StorageLocation = "C3-D2",
                        Description = "27-tums 4K UHD-skärm med HDR-stöd och USB-C-anslutning. Perfekt för design, video och professionell användning.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "KEYBOARD-LOGITECH-004", 
                        Name = "Logitech MX Keys Tangentbord", 
                        PriceInCents = 119900, // 1,199 kr
                        StockQuantity = 40, 
                        MinimumStock = 15,
                        StorageLocation = "D1-E3",
                        Description = "Trådlöst tangentbord med bakbelysning och smart belysning. Kompatibelt med Windows, Mac och Linux.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "MOUSE-RAZER-005", 
                        Name = "Razer DeathAdder V3 Gaming Mus", 
                        PriceInCents = 79900, // 799 kr
                        StockQuantity = 30, 
                        MinimumStock = 12,
                        StorageLocation = "E2-F1",
                        Description = "Ergonomisk gaming-mus med 30,000 DPI-sensor och RGB-belysning. Perfekt för esport och professionellt gaming.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "SSD-SAMSUNG-006", 
                        Name = "Samsung 980 PRO 1TB NVMe SSD", 
                        PriceInCents = 139900, // 1,399 kr
                        StockQuantity = 20, 
                        MinimumStock = 8,
                        StorageLocation = "F3-A2",
                        Description = "Snabb NVMe M.2 SSD med 7,000 MB/s läshastighet. Idealisk för gaming och krävande applikationer.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new Article 
                    { 
                        Sku = "RAM-CORSAIR-007", 
                        Name = "Corsair Vengeance 32GB DDR4", 
                        PriceInCents = 179900, // 1,799 kr
                        StockQuantity = 12, 
                        MinimumStock = 5,
                        StorageLocation = "G1-H3",
                        Description = "32GB DDR4-3200 RAM-kit (2x16GB) för höga prestanda. Perfekt för gaming, videobearbetning och multitasking.",
                        IsActive = true, 
                        CreatedUtc = DateTime.UtcNow 
                    }
                );
                // Spara artiklar först innan vi använder dem i order
                await context.SaveChangesAsync();
            }

            // 4. Seed order med realistiska datorbeställningar
            if (!context.Orders.Any())
            {
                var coordinatorFromDb = await userManager.FindByEmailAsync(orderCoordinator.Email);
                var employeeFromDb = await userManager.FindByEmailAsync(employee.Email);

                if (coordinatorFromDb == null || employeeFromDb == null)
                {
                    throw new Exception("Kunde inte hitta användare för att skapa ordrar.");
                }

                var coordinatorId = coordinatorFromDb.Id;
                var employeeId = employeeFromDb.Id;

                var order1 = new Order 
                { 
                    UserId = coordinatorId, 
                    Status = GlobalRules.OrderStatus.Processing, 
                    ExternalOrderNo = "ORD-2024-001",
                    CreatedUtc = DateTime.UtcNow.AddDays(-3), 
                    ConfirmedUtc = DateTime.UtcNow.AddDays(-2),
                    Notes = "Företagsbeställning för kontorsutrustning - Dell laptop och tillbehör" 
                };
                var order2 = new Order 
                { 
                    UserId = employeeId, 
                    Status = GlobalRules.OrderStatus.Created, 
                    ExternalOrderNo = "ORD-2024-002",
                    CreatedUtc = DateTime.UtcNow.AddHours(-5), 
                    Notes = "Gaming-setup beställning - Desktop, monitor och peripherals" 
                };

                context.Orders.AddRange(order1, order2);
                // Spara order först så vi får OrderId
                await context.SaveChangesAsync();

                // 5-8. Seed relaterad orderdata i en transaktion
                var articles = context.Articles.ToList();
                
                // OrderItems
                context.OrderItems.AddRange(
                    // Order 1: Företagsbeställning (Dell laptop + tillbehör)
                    new OrderItem { OrderId = order1.OrderId, ArticleId = articles[0].ArticleId, Quantity = 3, UnitPriceInCents = articles[0].PriceInCents }, // Dell XPS 13
                    new OrderItem { OrderId = order1.OrderId, ArticleId = articles[3].ArticleId, Quantity = 3, UnitPriceInCents = articles[3].PriceInCents }, // Logitech tangentbord
                    new OrderItem { OrderId = order1.OrderId, ArticleId = articles[4].ArticleId, Quantity = 3, UnitPriceInCents = articles[4].PriceInCents }, // Razer mus
                    
                    // Order 2: Gaming-setup
                    new OrderItem { OrderId = order2.OrderId, ArticleId = articles[1].ArticleId, Quantity = 1, UnitPriceInCents = articles[1].PriceInCents }, // HP Desktop
                    new OrderItem { OrderId = order2.OrderId, ArticleId = articles[2].ArticleId, Quantity = 1, UnitPriceInCents = articles[2].PriceInCents }, // Samsung Monitor
                    new OrderItem { OrderId = order2.OrderId, ArticleId = articles[5].ArticleId, Quantity = 1, UnitPriceInCents = articles[5].PriceInCents }, // Samsung SSD
                    new OrderItem { OrderId = order2.OrderId, ArticleId = articles[6].ArticleId, Quantity = 1, UnitPriceInCents = articles[6].PriceInCents }  // Corsair RAM
                );

                // Deliveries
                context.Deliveries.AddRange(
                    new Delivery 
                    { 
                        OrderId = order1.OrderId, 
                        Status = GlobalRules.DeliveryStatus.Shipped,
                        TrackingNumber = "DHL-SE-2024-001234",
                        EstimatedDeliveryUtc = DateTime.UtcNow.AddDays(1), 
                        DeliveryMethod = "Företagsleverans",
                        DeliveryAddress = "TechnoAB, Storgatan 15, 111 20 Stockholm",
                        Notes = "Leverans till reception, kontakta Erik Johansson vid ankomst",
                        CreatedUtc = DateTime.UtcNow.AddDays(-2),
                        ShippedUtc = DateTime.UtcNow.AddDays(-1)
                    },
                    new Delivery 
                    { 
                        OrderId = order2.OrderId, 
                        Status = GlobalRules.DeliveryStatus.Preparing,
                        EstimatedDeliveryUtc = DateTime.UtcNow.AddDays(3), 
                        DeliveryMethod = "Hemleverans",
                        DeliveryAddress = "Maria Andersson, Hemgatan 42, 123 45 Göteborg",
                        Notes = "Ring innan leverans",
                        CreatedUtc = DateTime.UtcNow.AddHours(-3)
                    }
                );

                // Stock Movements
                context.StockMovements.AddRange(
                    // Initial stock för Dell laptops
                    new StockMovement 
                    { 
                        ArticleId = articles[0].ArticleId, 
                        UserId = employeeId, 
                        MovementType = StockMovement.MovementTypes.Initial, 
                        Quantity = 20, 
                        StockAfterMovement = 20, 
                        Reason = "Första leverans från Dell", 
                        Notes = "Leverans av 20 Dell XPS 13 laptops från leverantör",
                        CreatedUtc = DateTime.UtcNow.AddDays(-10) 
                    },
                    // Inleverans av mer lager
                    new StockMovement 
                    { 
                        ArticleId = articles[1].ArticleId, 
                        UserId = employeeId, 
                        MovementType = StockMovement.MovementTypes.Incoming, 
                        Quantity = 10, 
                        StockAfterMovement = 18, 
                        Reason = "Återfyllnad från HP", 
                        Notes = "Ny leverans av HP Pavilion desktop-datorer",
                        CreatedUtc = DateTime.UtcNow.AddDays(-5) 
                    },
                    // Reserverat för order
                    new StockMovement 
                    { 
                        ArticleId = articles[0].ArticleId, 
                        UserId = coordinatorId, 
                        MovementType = StockMovement.MovementTypes.Reserved, 
                        Quantity = -3, 
                        StockAfterMovement = 17, 
                        OrderId = order1.OrderId,
                        Reason = "Reserverat för företagsorder ORD-2024-001", 
                        CreatedUtc = DateTime.UtcNow.AddDays(-2) 
                    }
                );

                // Order History
                context.OrderHistories.AddRange(
                    new OrderHistory 
                    { 
                        OrderId = order1.OrderId, 
                        ChangedByUserId = coordinatorId, 
                        NewStatus = GlobalRules.OrderStatus.Created, 
                        Comment = "Företagsbeställning mottagen från TechnoAB", 
                        ChangedUtc = DateTime.UtcNow.AddDays(-3) 
                    },
                    new OrderHistory 
                    { 
                        OrderId = order1.OrderId, 
                        ChangedByUserId = coordinatorId, 
                        OldStatus = GlobalRules.OrderStatus.Created,
                        NewStatus = GlobalRules.OrderStatus.Confirmed, 
                        Comment = "Order bekräftad, alla artiklar i lager", 
                        ChangedUtc = DateTime.UtcNow.AddDays(-2) 
                    },
                    new OrderHistory 
                    { 
                        OrderId = order1.OrderId, 
                        ChangedByUserId = employeeId, 
                        OldStatus = GlobalRules.OrderStatus.Confirmed,
                        NewStatus = GlobalRules.OrderStatus.Processing, 
                        Comment = "Plockning påbörjad i lager", 
                        ChangedUtc = DateTime.UtcNow.AddDays(-1) 
                    },
                    new OrderHistory 
                    { 
                        OrderId = order2.OrderId, 
                        ChangedByUserId = employeeId, 
                        NewStatus = GlobalRules.OrderStatus.Created, 
                        Comment = "Gaming-setup beställning registrerad", 
                        ChangedUtc = DateTime.UtcNow.AddHours(-5) 
                    }
                );

                // Spara all orderrelaterad data i en transaktion
                await context.SaveChangesAsync();
            }
        }
    }
}