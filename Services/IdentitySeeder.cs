using Microsoft.AspNetCore.Identity;
using WebShop.Data;
using WebShop.Models;

namespace WebShop.Services;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var adminEmail = configuration["AdminSeed:Email"] ?? "admin@webshop.local";
        var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        if (!await userManager.IsInRoleAsync(adminUser, "User"))
        {
            await userManager.AddToRoleAsync(adminUser, "User");
        }

        await EnsureDemoUserAsync(userManager, "user1@webshop.local", "User123!");
        await EnsureDemoUserAsync(userManager, "user2@webshop.local", "User123!");

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var demoCatalog = new List<Product>
        {
            new Product { Name = "Keyboard", Price = 499m, Description = "Simple mechanical keyboard" },
            new Product { Name = "Mouse", Price = 299m, Description = "Wireless mouse" },
            new Product { Name = "Monitor", Price = 1999m, Description = "24-inch monitor" },
            new Product { Name = "USB-C Hub", Price = 399m, Description = "7-in-1 hub for laptop" },
            new Product { Name = "Webcam", Price = 649m, Description = "Full HD webcam for meetings" },
            new Product { Name = "Laptop Stand", Price = 279m, Description = "Aluminum adjustable stand" },
            new Product { Name = "Gaming Headset", Price = 899m, Description = "Comfort headset with noise-cancel mic" },
            new Product { Name = "Desk Lamp", Price = 249m, Description = "LED lamp with adjustable brightness" },
            new Product { Name = "External SSD 1TB", Price = 1299m, Description = "Fast USB-C storage for backups" }
        };

        var existingNames = dbContext.Products.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingProducts = demoCatalog.Where(p => !existingNames.Contains(p.Name)).ToList();
        if (missingProducts.Count > 0)
        {
            dbContext.Products.AddRange(missingProducts);
            await dbContext.SaveChangesAsync();
        }

        var demoUser1 = await userManager.FindByEmailAsync("user1@webshop.local");
        var demoUser2 = await userManager.FindByEmailAsync("user2@webshop.local");

        await EnsureDemoOrdersForUserAsync(dbContext, demoUser1?.Id, "Processing", DateTime.UtcNow.AddMinutes(-45), [1, 2]);
        await EnsureDemoOrdersForUserAsync(dbContext, demoUser2?.Id, "Completed", DateTime.UtcNow.AddHours(-4), [2, 1]);
        await EnsureDemoOrdersForUserAsync(dbContext, demoUser1?.Id, "Pending", DateTime.UtcNow.AddMinutes(-10), [1, 1, 1]);
    }

    private static async Task EnsureDemoOrdersForUserAsync(
        ApplicationDbContext dbContext,
        string? userId,
        string status,
        DateTime createdAtUtc,
        IReadOnlyList<int> quantities)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (dbContext.Orders.Any(o => o.UserId == userId && o.Status == status))
        {
            return;
        }

        var products = dbContext.Products.OrderBy(p => p.Id).Take(quantities.Count).ToList();
        if (products.Count == 0)
        {
            return;
        }

        var items = new List<OrderItem>();
        for (var i = 0; i < products.Count; i++)
        {
            items.Add(new OrderItem
            {
                ProductId = products[i].Id,
                ProductName = products[i].Name,
                UnitPrice = products[i].Price,
                Quantity = quantities[i]
            });
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAtUtc = createdAtUtc,
            Status = status,
            Items = items,
            TotalAmount = items.Sum(i => i.UnitPrice * i.Quantity)
        };

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureDemoUserAsync(UserManager<IdentityUser> userManager, string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, "User"))
        {
            await userManager.AddToRoleAsync(user, "User");
        }
    }
}
