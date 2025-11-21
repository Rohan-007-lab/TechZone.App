using TechZone.Domain.Enums;

namespace TechZone.Application.DTOs
{
    /// <summary>
    /// Product Response DTO
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Images { get; set; } = new();
        public ProductStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Create Product Request DTO
    /// </summary>
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
    }

    /// <summary>
    /// Update Product Request DTO
    /// </summary>
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public ProductStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
    }

    /// <summary>
    /// Product List Item DTO (for list views)
    /// </summary>
    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFeatured { get; set; }
    }

    /// <summary>
    /// Product Filter DTO
    /// </summary>
    public class ProductFilterDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; }
        public bool? IsFeatured { get; set; }
        public ProductStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "Name";
        public string? SortOrder { get; set; } = "ASC";
    }
}