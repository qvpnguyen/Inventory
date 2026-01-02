using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Hubs;
using Inventory.Api.Persistence;
using Inventory.Api.Services;
using Inventory.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Inventory.Tests.Hubs
{
    public class OrderHubTests 
    {
        [Fact]
        public async Task CreateOrder_ShouldNotifyClients()
        {
            // Arrange
            var context = DbContextFactory.CreateInMemoryDbContext();

            var proxy = new Mock<IClientProxy>();
            var clients = new Mock<IHubClients>();
            var hubContext = new Mock<IHubContext<OrderHub>>();

            clients.Setup(c => c.All).Returns(proxy.Object);
            hubContext.Setup(h => h.Clients).Returns(clients.Object);

            var logger = new Mock<ILogger<OrderService>>().Object;
            var service = new OrderService(context, hubContext.Object, logger);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = "hashed"
            };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                StockQuantity = 10,
                Price = 50m,
                UserId = user.Id
            };

            context.Users.Add(user);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var request = new CreateOrderRequest
            {
                Items = new List<CreateOrderItemRequest>
                {
                    new() { ProductId = product.Id, Quantity = 2 },
                }
            };

            // Act
            await service.CreateAsync(user.Id, request);

            // Assert
            proxy.Verify(
                p => p.SendCoreAsync(
                    "OrderCreated",
                    It.Is<object[]>(args =>
                        args.Length == 1 &&
                        args[0] is OrderResponse &&
                        ((OrderResponse)args[0]).Items.Count == 1 &&
                        ((OrderResponse)args[0]).Items[0].ProductId == product.Id &&
                        ((OrderResponse)args[0]).Items[0].Quantity == 2
                        ),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
        }
    }
}
