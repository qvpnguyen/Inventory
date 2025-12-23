using Inventory.Api.Domain.Entities;
using Inventory.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace Inventory.Tests.Helpers
{
    public static class DbContextFactory
    {
        private static string GetConnectionString()
        {
            var dbName = $"inventorytest_{Guid.NewGuid():N}";
            return $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
        } 
        // For OrderServiceTests
        public static AppDbContext CreateDbContext() 
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted(); // reset
            context.Database.EnsureCreated();
            return context;
        }
        // For OrderHubTests
        public static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w =>
                    w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        public static User CreateTestUser(AppDbContext context)
        {
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = $"test{Guid.NewGuid()}@example.com",
                PasswordHash = "hashed"
            };

            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public static Product CreateTestProduct(AppDbContext context, User user, int stock = 5)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                StockQuantity = stock,
                Price = 50m,
                UserId = user.Id
            };

            context.Products.Add(product);
            context.SaveChanges();
            return product;
        }
    }
}
