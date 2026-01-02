using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Hubs;
using Inventory.Api.Persistence;
using Inventory.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Inventory.Api.Exceptions;

namespace Inventory.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, IHubContext<OrderHub> hubContext, ILogger<OrderService> logger)
        {
            _context = context;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<Order> CreateAsync(Guid userId, CreateOrderRequest request)
        {
            _logger.LogInformation(
                $"Starting order creation for user {userId} with {request.Items.Count} items");
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId && p.UserId == userId);

                    if (product is null)
                    {
                        _logger.LogWarning(
                            $"Product with id {item.ProductId} not found");
                        throw new NotFoundException($"Product with id {item.ProductId} not found");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        _logger.LogWarning(
                            $"Insufficient stock for product {product.Id}. Requested {item.Quantity}. Available {product.StockQuantity}");
                        throw new BusinessRuleException($"Insufficient stock for product {product.Id}. Requested {item.Quantity}. Available {product.StockQuantity}");
                    }

                    product.StockQuantity -= item.Quantity;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    });
                }

                order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // SignalR notification
                var orderDto = MapToResponse(order);
                await _hubContext.Clients.All.SendAsync("OrderCreated", orderDto);

                _logger.LogInformation(
                    $"Order {order.Id} successfully created for user {userId} (Total: {order.TotalAmount})");

                return order;
            } catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync(Guid userId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning(
                    $"Order {orderId} not found");
                throw new NotFoundException($"Order {orderId} not found");
            }
            if (order.UserId != userId)
            {
                _logger.LogWarning(
                    $"Access denied for user {userId}");
                throw new ForbiddenException($"Access denied for user {userId}");
            }
            return order;
        }

        public static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
