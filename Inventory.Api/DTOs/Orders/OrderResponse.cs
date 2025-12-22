namespace Inventory.Api.DTOs.Orders
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
