using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Persistence;
using Inventory.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateAsync(Guid userId, CreateOrderRequest request)
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
                    throw new KeyNotFoundException("Product not found");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock");
                }

                product.StockQuantity -= item.Quantity;

                order.Items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
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
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }
    }
}
