using Inventory.Api.Domain.Entities;
using Inventory.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET api/products/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyProducts()
        {
            // Get UserId from JWT
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var products = await _productService.GetProductsByUserAsync(userId);
            return Ok(products);
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Associate the product to the connected user
            product.UserId = userId;

            var createdProduct = await _productService.CreateAsync(product);
            return CreatedAtAction(nameof(GetMyProducts), new { id = createdProduct.Id }, createdProduct);
        }

        // PUT api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] Product updatedProduct)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.UserId != userId)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.StockQuantity = updatedProduct.StockQuantity;

            await _productService.UpdateAsync(product);

            return NoContent();
        }

        // DELETE api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var product = await _productService.GetByIdAsync(id);

            if (product == null || product.UserId != userId)
                return NotFound();

            await _productService.DeleteAsync(product);

            return NoContent();
        }
    }
}
