namespace Inventory.Api.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
