using TechZone.Domain.Common;
using TechZone.Domain.Enums;

namespace TechZone.Domain.Entities
{
    /// <summary>
    /// Product Entity
    /// </summary>
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrl { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public bool IsFeatured { get; set; } = false;
        public decimal? Weight { get; set; }
        public string? Dimensions { get; set; }
        public int ViewCount { get; set; } = 0;
        public decimal AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; } = 0;

        // Navigation Properties
        public Category Category { get; set; } = null!;
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }

    /// <summary>
    /// Product Images Entity
    /// </summary>
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public Product Product { get; set; } = null!;
    }
}