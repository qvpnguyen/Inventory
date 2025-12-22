namespace Inventory.Api.DTOs.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
