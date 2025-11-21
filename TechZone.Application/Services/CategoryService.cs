using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Interfaces;

namespace TechZone.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                ParentCategoryId = c.ParentCategoryId,
                IsActive = c.IsActive,
                ProductCount = products.Count(p => p.CategoryId == c.Id)
            }).ToList();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return null;

            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.CategoryId == id);

            Category? parentCategory = null;
            if (category.ParentCategoryId.HasValue)
            {
                parentCategory = await _unitOfWork.Repository<Category>()
                    .GetByIdAsync(category.ParentCategoryId.Value);
            }

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = parentCategory?.Name,
                IsActive = category.IsActive,
                ProductCount = products.Count()
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = dto.IsActive,
                DisplayOrder = 0
            };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return await GetCategoryByIdAsync(category.Id) ?? new CategoryDto();
        }

        public async Task<CategoryDto> UpdateCategoryAsync(CategoryDto dto)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(dto.Id);
            if (category == null)
                throw new Exception("Category not found");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageUrl = dto.ImageUrl;
            category.ParentCategoryId = dto.ParentCategoryId;
            category.IsActive = dto.IsActive;

            await _unitOfWork.Repository<Category>().UpdateAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return await GetCategoryByIdAsync(category.Id) ?? new CategoryDto();
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return false;

            // Check if category has products
            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.CategoryId == id);

            if (products.Any())
                throw new Exception("Cannot delete category with existing products");

            // Check if category has subcategories
            var subCategories = await _unitOfWork.Repository<Category>()
                .FindAsync(c => c.ParentCategoryId == id);

            if (subCategories.Any())
                throw new Exception("Cannot delete category with subcategories");

            await _unitOfWork.Repository<Category>().DeleteAsync(category);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}