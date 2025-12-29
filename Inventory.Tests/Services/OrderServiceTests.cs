using FluentAssertions;
using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Hubs;
using Inventory.Api.Services;
using Inventory.Api.Exceptions;
using Inventory.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration.UserSecrets;
using Moq;
using Xunit;

namespace Inventory.Tests.Services
{
    public class OrderServiceTests
    {
        private static IHubContext<OrderHub> CreateMockHub()
        {
            var hub = new Mock<IHubContext<OrderHub>>();
            var clients = new Mock<IHubClients>();
            var proxy = new Mock<IClientProxy>();

            hub.Setup(h => h.Clients).Returns(clients.Object);
            clients.Setup(c => c.All).Returns(proxy.Object);

            return hub.Object;
        }

        [Fact]
        public async Task CreateOrder_ShouldCreateOrder_AndDecrementStock()
        {
            // Arrange
            using var context = DbContextFactory.CreateDbContext();
            var user = DbContextFactory.CreateTestUser(context);
            var product = DbContextFactory.CreateTestProduct(context, user, stock: 5);
            var service = new OrderService(context, CreateMockHub());

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product.Id, Quantity = 2 }
                }
            };

            // Act
            var order = await service.CreateAsync(user.Id, request);

            // Assert
            var updatedProduct = await context.Products.FindAsync(product.Id);
            Assert.Equal(3, updatedProduct!.StockQuantity);
        }

        [Fact]
        public async Task CreateOrder_ProductNotFound_ShouldThrow()
        {
            // Arrange
            using var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

            var user = DbContextFactory.CreateTestUser(context);
            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = Guid.NewGuid(), Quantity = 1 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                service.CreateAsync(user.Id, request));
        }

        [Fact]
        public async Task CreateOrder_InsufficientStock_ShouldThrow()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

            var user = DbContextFactory.CreateTestUser(context);


            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Limited Product",
                StockQuantity = 1,
                Price = 10m,
                UserId = user.Id
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product.Id, Quantity = 100 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                service.CreateAsync(user.Id, request));

            // Stock shouldn't have changed
            (await context.Products.FindAsync(product.Id))!.StockQuantity.Should().Be(1);
        }

        [Fact]
        public async Task CreateOrder_WhenOneItemFails_ShouldRollbackAll()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

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
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                service.CreateAsync(user.Id, request));

            // Refresh entities of current context
            await context.Entry(product1).ReloadAsync();
            await context.Entry(product2).ReloadAsync();

            // Assert : Product1 stock has not been changed
            (await context.Products.FindAsync(product1.Id))!.StockQuantity.Should().Be(5);
            (await context.Products.FindAsync(product2.Id))!.StockQuantity.Should().Be(0);
        }

        [Fact]
        public async Task CreateOrder_WhenOneItemDoesntExist_ShouldRollbackAll()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

            var user = DbContextFactory.CreateTestUser(context);

            var product1 = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                StockQuantity = 5,
                Price = 20m,
                UserId = user.Id
            };
            var product2Id = Guid.NewGuid();

            context.Products.AddRange(product1);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product1.Id, Quantity = 2 },
                    new() { ProductId = product2Id, Quantity = 1 }
                }
            };

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.CreateAsync(user.Id, request));

            // Refresh entities of current context
            await context.Entry(product1).ReloadAsync();

            // Assert : Product1 stock has not been changed
            (await context.Products.FindAsync(product1.Id))!.StockQuantity.Should().Be(5);
            (await context.Products.FindAsync(product2Id)).Should().BeNull();
        }

        [Fact]
        public async Task GetOrder_ByOwner_ShouldSucceed()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

            var owner = DbContextFactory.CreateTestUser(context);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                StockQuantity = 5,
                Price = 20m,
                UserId = owner.Id
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 40m,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = 2,
                        UnitPrice = product.Price
                    }
                }
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetByIdAsync(owner.Id, order.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(order.Id);
            result.UserId.Should().Be(owner.Id);
            result.Items.Should().HaveCount(1);
            result.Items.ElementAt(0).ProductId.Should().Be(product.Id);
            result.Items.ElementAt(0).Quantity.Should().Be(2);
            result.Items.ElementAt(0).UnitPrice.Should().Be(product.Price);
        }

        [Fact]
        public async Task GetOrder_ByIntruder_ShouldThrowForbiddenException()
        {
            // Arrange
            var context = DbContextFactory.CreateDbContext();
            var service = new OrderService(context, CreateMockHub());

            var owner = DbContextFactory.CreateTestUser(context);
            var intruder = DbContextFactory.CreateTestUser(context);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                StockQuantity = 5,
                Price = 20m,
                UserId = owner.Id
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = owner.Id,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 40m,
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = product.Id,
                Quantity = 2,
                UnitPrice = product.Price
            }
        }
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() =>
                service.GetByIdAsync(intruder.Id, order.Id));
        }

    }
}
