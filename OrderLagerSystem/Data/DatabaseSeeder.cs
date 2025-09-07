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

            // 2. Seed användare
            var admin = new ApplicationUser 
            { 
                UserName = "admin@demo.se", 
                Email = "admin@demo.se", 
                EmailConfirmed = true, 
                FullName = "Admin User", 
                FirstName = "Admin", 
                LastName = "User", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };
            var user1 = new ApplicationUser 
            { 
                UserName = "user1@demo.se", 
                Email = "user1@demo.se", 
                EmailConfirmed = true, 
                FullName = "User One", 
                FirstName = "User", 
                LastName = "One", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };
            var user2 = new ApplicationUser 
            { 
                UserName = "user2@demo.se", 
                Email = "user2@demo.se", 
                EmailConfirmed = true, 
                FullName = "User Two", 
                FirstName = "User", 
                LastName = "Two", 
                IsActive = true, 
                CreatedUtc = DateTime.UtcNow 
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, GlobalRules.Roles.Admin);
            }

            if (await userManager.FindByEmailAsync(user1.Email) == null)
            {
                await userManager.CreateAsync(user1, "User123!");
                await userManager.AddToRoleAsync(user1, GlobalRules.Roles.Orderkoordinator);
            }

            if (await userManager.FindByEmailAsync(user2.Email) == null)
            {
                await userManager.CreateAsync(user2, "User123!");
                await userManager.AddToRoleAsync(user2, GlobalRules.Roles.Employee);
            }

            // 3. Seed artiklar
            if (!context.Articles.Any())
            {
                context.Articles.AddRange(
                    new Article { Sku = "SKU001", Name = "Artikel 1", PriceInCents = 10000, StockQuantity = 10, Description = "Beskrivning av Artikel 1", IsActive = true, CreatedUtc = DateTime.UtcNow },
                    new Article { Sku = "SKU002", Name = "Artikel 2", PriceInCents = 20000, StockQuantity = 5, Description = "Beskrivning av Artikel 2", IsActive = true, CreatedUtc = DateTime.UtcNow },
                    new Article { Sku = "SKU003", Name = "Artikel 3", PriceInCents = 15000, StockQuantity = 8, Description = "Beskrivning av Artikel 3", IsActive = true, CreatedUtc = DateTime.UtcNow },
                    new Article { Sku = "SKU004", Name = "Artikel 4", PriceInCents = 30000, StockQuantity = 2, Description = "Beskrivning av Artikel 4", IsActive = true, CreatedUtc = DateTime.UtcNow },
                    new Article { Sku = "SKU005", Name = "Artikel 5", PriceInCents = 5000, StockQuantity = 20, Description = "Beskrivning av Artikel 5", IsActive = true, CreatedUtc = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            // 4. Seed orders
            if (!context.Orders.Any())
            {
                var user1FromDb = await userManager.FindByEmailAsync(user1.Email);
                var user2FromDb = await userManager.FindByEmailAsync(user2.Email);

                if (user1FromDb == null || user2FromDb == null)
                {
                    throw new Exception("Kunde inte hitta användare för att skapa ordrar.");
                }

                var user1Id = user1FromDb.Id;
                var user2Id = user2FromDb.Id;

                var order1 = new Order { UserId = user1Id, Status = GlobalRules.OrderStatus.Created, CreatedUtc = DateTime.UtcNow, Notes = "Första ordern" };
                var order2 = new Order { UserId = user2Id, Status = GlobalRules.OrderStatus.Created, CreatedUtc = DateTime.UtcNow, Notes = "Andra ordern" };

                context.Orders.AddRange(order1, order2);
                await context.SaveChangesAsync();

                // 5. Seed orderitems
                var articles = context.Articles.ToList();
                context.OrderItems.AddRange(
                    new OrderItem { OrderId = order1.OrderId, ArticleId = articles[0].ArticleId, Quantity = 2, UnitPriceInCents = articles[0].PriceInCents },
                    new OrderItem { OrderId = order1.OrderId, ArticleId = articles[1].ArticleId, Quantity = 1, UnitPriceInCents = articles[1].PriceInCents },
                    new OrderItem { OrderId = order2.OrderId, ArticleId = articles[2].ArticleId, Quantity = 3, UnitPriceInCents = articles[2].PriceInCents }
                );
                await context.SaveChangesAsync();

                // 6. Seed delivery
                context.Deliveries.Add(new Delivery 
                { 
                    OrderId = order1.OrderId, 
                    Status = GlobalRules.DeliveryStatus.Pending, 
                    EstimatedDeliveryUtc = DateTime.UtcNow.AddDays(2), 
                    DeliveryMethod = "Hemleverans", 
                    CreatedUtc = DateTime.UtcNow 
                });
                await context.SaveChangesAsync();

                // 7. Seed stock movements
                context.StockMovements.AddRange(
                    new StockMovement 
                    { 
                        ArticleId = articles[0].ArticleId, 
                        UserId = user1Id, 
                        MovementType = StockMovement.MovementTypes.Initial, 
                        Quantity = 10, 
                        StockAfterMovement = 10, 
                        Reason = "Initial stock setup", 
                        CreatedUtc = DateTime.UtcNow 
                    },
                    new StockMovement 
                    { 
                        ArticleId = articles[1].ArticleId, 
                        UserId = user1Id, 
                        MovementType = StockMovement.MovementTypes.Initial, 
                        Quantity = 5, 
                        StockAfterMovement = 5, 
                        Reason = "Initial stock setup", 
                        CreatedUtc = DateTime.UtcNow 
                    }
                );
                await context.SaveChangesAsync();

                // 8. Seed order history
                context.OrderHistories.Add(new OrderHistory 
                { 
                    OrderId = order1.OrderId, 
                    ChangedByUserId = user1Id, 
                    NewStatus = GlobalRules.OrderStatus.Created, 
                    Comment = "Order skapad", 
                    ChangedUtc = DateTime.UtcNow 
                });
                await context.SaveChangesAsync();
            }
        }
    }
}