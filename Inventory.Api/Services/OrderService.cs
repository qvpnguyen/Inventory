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

        public OrderService(AppDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<Order> CreateAsync(Guid userId, CreateOrderRequest request)
        {
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
                        throw new NotFoundException($"Product with id {item.ProductId} not found");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new BusinessRuleException($"Insufficient stock for product {product.Name}");
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
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid userId, Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new NotFoundException("Order not found");
            }
            if (order.UserId != userId)
            {
                throw new ForbiddenException("Access denied");
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
