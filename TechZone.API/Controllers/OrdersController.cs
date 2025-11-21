using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;

namespace TechZone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderDto>>>> GetAllOrders(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _orderService.GetAllOrdersAsync(pageNumber, pageSize);
                return Ok(ApiResponse<PagedResponse<OrderDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResponse<OrderDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                    return NotFound(ApiResponse<OrderDto>.ErrorResponse("Order not found"));

                // Users can only view their own orders, admins can view all
                if (!User.IsInRole("Admin") && order.UserId != userId)
                    return Forbid();

                return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get current user's orders
        /// </summary>
        [HttpGet("my-orders")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<List<OrderDto>>.ErrorResponse("User not authenticated"));

                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<OrderDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<OrderDto>.ErrorResponse("User not authenticated"));

                dto.UserId = userId;
                var order = await _orderService.CreateOrderAsync(dto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id },
                    ApiResponse<OrderDto>.SuccessResponse(order, "Order created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<OrderDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            try
            {
                if (id != dto.OrderId)
                    return BadRequest(ApiResponse<bool>.ErrorResponse("ID mismatch"));

                var result = await _orderService.UpdateOrderStatusAsync(dto);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Order not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Order status updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelOrder(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var order = await _orderService.GetOrderByIdAsync(id);

                if (order == null)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Order not found"));

                // Users can only cancel their own orders
                if (!User.IsInRole("Admin") && order.UserId != userId)
                    return Forbid();

                var result = await _orderService.CancelOrderAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Order cancelled successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}