using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Products;
using Inventory.Api.Persistence;
using Inventory.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;

        public ProductService(AppDbContext context, ILogger<ProductService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Product> CreateAsync(Guid userId, CreateProductRequest request)
        {
            _logger.LogInformation(
                $"Creating product '{request.Name}' for user {userId}");
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                UserId = userId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                $"Product {product.Id} '{product.Name}' successfully created by user {userId}");
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(Guid userId)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByUserAsync(Guid userId)
        {
            return await _context.Products
                                 .AsNoTracking()
                                 .Where(p => p.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid userId, Guid productId)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
        }

        public async Task UpdateAsync(Guid userId, Guid productId, UpdateProductRequest request)
        {
            _logger.LogInformation(
                $"Updating product {productId} for user {userId}");
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product is null)
            {
                _logger.LogWarning(
                    $"Product with id {productId} not found");
                throw new KeyNotFoundException("Product not found");
            }

            product.Name = request.Name;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                $"Product {product.Id} successfully updated by user {userId}");
            _logger.LogDebug(
                $"Updated product values: Name={product.Name}, Price={product.Price}, Stock={product.StockQuantity}");
        }

        public async Task DeleteAsync(Guid userId, Guid productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId ==  userId);

            if (product is null)
            {
                _logger.LogWarning(
                    $"Product with id {productId} not found");
                throw new KeyNotFoundException($"Product with id {productId} not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
            };
        }
    }
}
