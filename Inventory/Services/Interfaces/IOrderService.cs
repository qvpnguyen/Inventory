using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateAsync(Order order);
        Task<List<Order>> GetOrdersByUserAsync(Guid userId);
        Task<Order?> GetByIdAsync(Guid id, Guid userId);
        Task UpdateAsync(Order order);
        Task DeleteAsync(Order order);
    }
}
