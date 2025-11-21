using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;

namespace TechZone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProductListDto>>>> GetProducts([FromQuery] ProductFilterDto filter)
        {
            try
            {
                var result = await _productService.GetProductsAsync(filter);
                return Ok(ApiResponse<PagedResponse<ProductListDto>>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PagedResponse<ProductListDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                    return NotFound(ApiResponse<ProductDto>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get featured products
        /// </summary>
        [HttpGet("featured")]
        public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> GetFeaturedProducts([FromQuery] int count = 10)
        {
            try
            {
                var products = await _productService.GetFeaturedProductsAsync(count);
                return Ok(ApiResponse<List<ProductListDto>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductListDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                return Ok(ApiResponse<List<ProductListDto>>.SuccessResponse(products));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductListDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
        {
            try
            {
                var product = await _productService.CreateProductAsync(dto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id },
                    ApiResponse<ProductDto>.SuccessResponse(product, "Product created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update product (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponse<ProductDto>.ErrorResponse("ID mismatch"));

                var product = await _productService.UpdateProductAsync(dto);
                return Ok(ApiResponse<ProductDto>.SuccessResponse(product, "Product updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete product (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Product not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}