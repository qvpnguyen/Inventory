using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs.Products
{
    public class CreateProductRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be zero or greater")]
        public int StockQuantity { get; set; }
    }
}
