using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Products;

namespace Inventory.Api.Services.Interfaces
{
    public interface IProductService
    {
        // Create a product
        Task<Product> CreateAsync(Guid userId, CreateProductRequest request);

        // Get all products
        Task<IEnumerable<Product>> GetAllAsync(Guid userId);

        //Get products by user
        Task<IEnumerable<Product>> GetProductsByUserAsync(Guid userId);

        // Get product by id
        Task<Product?> GetByIdAsync(Guid userId, Guid productId);

        // Update an existing product
        Task UpdateAsync(Guid userId, Guid productId, UpdateProductRequest request);

        // Delete a product
        Task DeleteAsync(Guid userId, Guid productId);
    }
}
