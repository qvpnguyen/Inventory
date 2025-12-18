using Inventory.Api.Domain.Entities;

namespace Inventory.Api.Services.Interfaces
{
    public interface IProductService
    {
        // Create a product
        Task<Product> CreateAsync(Product product);

        // Get all products
        Task<List<Product>> GetAllAsync();

        //Get products by user
        Task<List<Product>> GetProductsByUserAsync(Guid userId);

        // Get product by id
        Task<Product?> GetByIdAsync(Guid id);

        // Update an existing product
        Task UpdateAsync(Product product);

        // Delete a product
        Task DeleteAsync(Product product);
    }
}
