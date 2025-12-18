namespace Inventory.Api.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null;
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
