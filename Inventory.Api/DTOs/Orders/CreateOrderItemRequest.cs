using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        [Required(ErrorMessage = "ProductId is required")]
        public Guid ProductId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
