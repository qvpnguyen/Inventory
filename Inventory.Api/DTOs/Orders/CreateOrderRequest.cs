using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs.Orders
{
    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "Order must have at least one item")]
        [MinLength(1, ErrorMessage = "Order must have at least one item")]
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
