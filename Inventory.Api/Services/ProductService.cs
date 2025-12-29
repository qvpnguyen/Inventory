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

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateAsync(Guid userId, CreateProductRequest request)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                UserId = userId,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(Guid userId)
        {
            return await _context.Products
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByUserAsync(Guid userId)
        {
            return await _context.Products
                                 .Where(p => p.UserId == userId)
                                 .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid userId, Guid productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);
        }

        public async Task UpdateAsync(Guid userId, Guid productId, UpdateProductRequest request)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId == userId);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            product.Name = request.Name;
            product.Price = request.Price;
            product.StockQuantity = request.StockQuantity;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid userId, Guid productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.UserId ==  userId);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found");
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
