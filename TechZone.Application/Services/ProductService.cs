using Microsoft.EntityFrameworkCore;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Enums;
using TechZone.Domain.Interfaces;

namespace TechZone.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResponse<ProductListDto>> GetProductsAsync(ProductFilterDto filter)
        {
            var productsRepo = _unitOfWork.Repository<Product>();
            var query = (await productsRepo.GetAllAsync()).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                        p.Description.Contains(filter.SearchTerm));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            if (!string.IsNullOrEmpty(filter.Brand))
            {
                query = query.Where(p => p.Brand == filter.Brand);
            }

            if (filter.IsFeatured.HasValue)
            {
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            // Sorting
            query = filter.SortBy?.ToLower() switch
            {
                "price" => filter.SortOrder == "DESC"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                "name" => filter.SortOrder == "DESC"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "rating" => filter.SortOrder == "DESC"
                    ? query.OrderByDescending(p => p.AverageRating)
                    : query.OrderBy(p => p.AverageRating),
                _ => query.OrderBy(p => p.Name)
            };

            // Get total count
            var totalRecords = query.Count();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)filter.PageSize);

            // Pagination
            var products = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var productDtos = products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                SKU = p.SKU,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsFeatured = p.IsFeatured
            }).ToList();

            return new PagedResponse<ProductListDto>
            {
                Items = productDtos,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords
            };
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                SKU = product.SKU,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                Brand = product.Brand,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                IsFeatured = product.IsFeatured,
                AverageRating = product.AverageRating,
                ReviewCount = product.ReviewCount,
                CreatedAt = product.CreatedAt
            };
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                ShortDescription = dto.ShortDescription,
                SKU = dto.SKU,
                Price = dto.Price,
                DiscountPrice = dto.DiscountPrice,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                Brand = dto.Brand,
                ImageUrl = dto.ImageUrl,
                IsFeatured = dto.IsFeatured,
                Weight = dto.Weight,
                Dimensions = dto.Dimensions,
                Status = ProductStatus.Active
            };

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id) ?? new ProductDto();
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.Id);
            if (product == null)
                throw new Exception("Product not found");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.ShortDescription = dto.ShortDescription;
            product.Price = dto.Price;
            product.DiscountPrice = dto.DiscountPrice;
            product.StockQuantity = dto.StockQuantity;
            product.CategoryId = dto.CategoryId;
            product.Brand = dto.Brand;
            product.ImageUrl = dto.ImageUrl;
            product.Status = dto.Status;
            product.IsFeatured = dto.IsFeatured;
            product.Weight = dto.Weight;
            product.Dimensions = dto.Dimensions;

            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return await GetProductByIdAsync(product.Id) ?? new ProductDto();
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            await _unitOfWork.Repository<Product>().DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(int count = 10)
        {
            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.IsFeatured && p.Status == ProductStatus.Active);

            return products.Take(count).Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                SKU = p.SKU,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsFeatured = p.IsFeatured
            }).ToList();
        }

        public async Task<List<ProductListDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.CategoryId == categoryId && p.Status == ProductStatus.Active);

            return products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                ShortDescription = p.ShortDescription,
                SKU = p.SKU,
                Price = p.Price,
                DiscountPrice = p.DiscountPrice,
                StockQuantity = p.StockQuantity,
                ImageUrl = p.ImageUrl,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                IsFeatured = p.IsFeatured
            }).ToList();
        }
    }
}