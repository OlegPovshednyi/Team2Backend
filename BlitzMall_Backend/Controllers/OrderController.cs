using BlitzMall_Backend.DTOs.Order;
using BlitzMall_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlitzMall_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var order = await _orderService.CreateFromCartAsync(dto);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Unexpected error." });
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
        {
            try
            {
                var orders = await _orderService.GetMyOrdersAsync();
                return Ok(orders);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Unexpected error." });
            }
        }
    }
}
