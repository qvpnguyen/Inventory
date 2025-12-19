using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;

namespace Inventory.Api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateAsync(Guid userId, CreateOrderRequest request);
        Task<IEnumerable<Order>> GetAllAsync(Guid userId);
        Task<Order?> GetByIdAsync(Guid userId, Guid orderId);
    }
}
