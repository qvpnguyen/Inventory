using Inventory.Api.Domain.Entities;
using Inventory.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace Inventory.Tests.Helpers
{
    public class DbContextFactory
    {
        public static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Database=inventory_api_test;Username=postgres;Password=postgres")
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureDeleted(); // reset
            context.Database.EnsureCreated();
            return context;
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
            //context.Users.Add(user);
            //context.SaveChanges();
            return user;
        }

    }
}
