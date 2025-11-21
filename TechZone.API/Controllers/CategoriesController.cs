using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;

namespace TechZone.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<CategoryDto>>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create new category (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CategoryDto dto)
        {
            try
            {
                var category = await _categoryService.CreateCategoryAsync(dto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id },
                    ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update category (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            try
            {
                if (id != dto.Id)
                    return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("ID mismatch"));

                var category = await _categoryService.UpdateCategoryAsync(dto);
                return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category updated successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CategoryDto>.ErrorResponse($"Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete category (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                    return NotFound(ApiResponse<bool>.ErrorResponse("Category not found"));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Category deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Error: {ex.Message}"));
            }
        }
    }
}