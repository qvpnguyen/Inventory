using Inventory.Api.Domain.Entities;
using Inventory.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Inventory.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // GET api/orders/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(orders);
        }

        // GET api/orders/my/{id}
        [HttpGet("my/{id}")]
        public async Task<IActionResult> GetMyOrder(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetByIdAsync(id, userId);
            if (order == null) return NotFound();
            return Ok(order);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            order.UserId = userId;

            var createdOrder = await _orderService.CreateAsync(order);
            return CreatedAtAction(nameof(GetMyOrder), new { id = createdOrder.Id }, createdOrder);
        }

        // PUT api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] Order updatedOrder)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetByIdAsync(id, userId);
            if (order == null) return NotFound();
            
            // Update order properties
            order.Items = updatedOrder.Items;

            await _orderService.UpdateAsync(order);
            return NoContent();
        }

        // DELETE api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetByIdAsync(id, userId);
            if (order == null) return NotFound();

            await _orderService.DeleteAsync(order);
            return NoContent();
        }
    }
}
