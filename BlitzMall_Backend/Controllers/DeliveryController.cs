using BlitzMall_Backend.DTOs.Delivery;
using BlitzMall_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlitzMall_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveryController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<DeliveryDto>>> GetAll()
        {
            try
            {
                return Ok(await _deliveryService.GetAllAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryDto>> GetById(int id)
        {
            try
            {
                var delivery = await _deliveryService.GetByIdAsync(id);

                return delivery == null
                    ? NotFound()
                    : Ok(delivery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<DeliveryDto>> Create(
            [FromBody] CreateDeliveryDto dto)
        {
            try
            {
                var delivery = await _deliveryService.CreateAsync(dto);

                return delivery == null
                    ? BadRequest(new { message = "Order not found." })
                    : CreatedAtAction(
                        nameof(GetById),
                        new { id = delivery.Id },
                        delivery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<DeliveryDto>> Update(
            int id,
            [FromBody] UpdateDeliveryDto dto)
        {
            try
            {
                var delivery = await _deliveryService.UpdateAsync(id, dto);

                return delivery == null
                    ? NotFound()
                    : Ok(delivery);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                return await _deliveryService.DeleteAsync(id)
                    ? NoContent()
                    : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
