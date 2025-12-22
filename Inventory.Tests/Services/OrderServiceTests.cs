using FluentAssertions;
using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Services;
using Inventory.Tests.Helpers;
using Microsoft.Extensions.Configuration.UserSecrets;
using Xunit;

namespace Inventory.Tests.Services
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task CreateOrder_ShouldCreateOrder_AndDecrementStock()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var orderService = new OrderService(context);

            var user = DbContextFactory.CreateTestUser(context);
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                StockQuantity = 10,
                Price = 49.99m,
                UserId = user.Id
            };
            context.Users.Add(user);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            };

            // Act
            var order = await orderService.CreateAsync(user.Id, request);

            // Assert
            order.Items.Count.Should().Be(1);
            order.Items.ElementAt(0).UnitPrice.Should().Be(49.99m);
            order.TotalAmount.Should().Be(99.98m);
            (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(8);
        }

        [Fact]
        public async Task CreateOrder_ProductNotFound_ShouldThrow()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var orderService = new OrderService(context);

            var user = DbContextFactory.CreateTestUser(context);
            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                orderService.CreateAsync(user.Id, request));
        }

        [Fact]
        public async Task CreateOrder_InsufficientStock_ShouldThrow()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var orderService = new OrderService(context);

            var user = DbContextFactory.CreateTestUser(context);


            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Limited Product",
                StockQuantity = 1,
                Price = 10m,
                UserId = user.Id
            };
            context.Users.Add(user);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                orderService.CreateAsync(user.Id, request));

            // Stock shouldn't have changed
            (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(1);
        }

        [Fact]
        public async Task CreateOrder_WhenOneItemFails_ShouldRollbackAll()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var orderService = new OrderService(context);

            var user = DbContextFactory.CreateTestUser(context);

            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                StockQuantity = 5,
                Price = 20m,
                UserId = user.Id
            };
            var product2 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                StockQuantity = 0,
                Price = 15m,
                UserId = user.Id
            };

            context.Users.Add(user);
            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product1.Id, Quantity = 2 },
                    new() { ProductId = product2.Id, Quantity = 1 }
                }
            };

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                orderService.CreateAsync(user.Id, request));

            // Refresh entities of current context
            await context.Entry(product1).ReloadAsync();
            await context.Entry(product2).ReloadAsync();

            // Assert : Product1 stock has not been changed
            (await context.Products.FindAsync(product1.Id))!.StockQuantity.Should().Be(5);
            (await context.Products.FindAsync(product2.Id))!.StockQuantity.Should().Be(0);
        }

    }
}
