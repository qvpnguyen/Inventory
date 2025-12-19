namespace Inventory.Api.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
