using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;

namespace TechZone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get current user's cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<CartDto>.ErrorResponse("User not authenticated"));

                var cart = await _cartService.GetUserCartAsync(userId);
                if (cart == null)
                    return Ok(ApiResponse<CartDto>.SuccessResponse(new CartDto { UserId = userId }));

                return Ok(ApiResponse<CartDto>.SuccessResponse(cart));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<CartDto>.ErrorResponse("User not authenticated"));

                dto.UserId = userId;
                var cart = await _cartService.AddToCartAsync(dto);
                return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Item added to cart"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem([FromBody] UpdateCartItemDto dto)
        {
            try
            {
                var cart = await _cartService.UpdateCartItemAsync(dto);
                return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Cart updated"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CartDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("remove/{cartItemId}")]
        public async Task<ActionResult<ApiResponse<bool>>> RemoveFromCart(int cartItemId)
        {
            try
            {
                var result = await _cartService.RemoveFromCartAsync(cartItemId);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Cart item not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Item removed from cart"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse<bool>>> ClearCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated"));

                var result = await _cartService.ClearCartAsync(userId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Cart cleared"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}