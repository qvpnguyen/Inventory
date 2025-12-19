namespace Inventory.Api.DTOs.Products
{
    public class UpdateProductRequest
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
