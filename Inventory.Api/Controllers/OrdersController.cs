using Inventory.Api.Domain.Entities;
using Inventory.Api.DTOs.Orders;
using Inventory.Api.Exceptions;
using Inventory.Api.Services;
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
            var orders = await _orderService.GetAllAsync(userId);
            return Ok(orders.Select(OrderService.MapToResponse));
        }

        // GET api/orders/my/{id}
        [HttpGet("my/{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.GetByIdAsync(userId, id);
            if (order == null) return NotFound();
            return Ok(OrderService.MapToResponse(order));
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = await _orderService.CreateAsync(userId, request);
            var response = OrderService.MapToResponse(order);

            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = response.Id },
                response);
        }
    }
}
