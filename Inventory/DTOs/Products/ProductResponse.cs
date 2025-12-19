namespace Inventory.Api.DTOs.Products
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
