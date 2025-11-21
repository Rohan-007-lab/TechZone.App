using TechZone.Domain.Common;

namespace TechZone.Domain.Entities
{
    /// <summary>
    /// Wishlist Entity
    /// </summary>
    public class Wishlist : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = "My Wishlist";

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }

    /// <summary>
    /// Wishlist Item Entity
    /// </summary>
    public class WishlistItem : BaseEntity
    {
        public int WishlistId { get; set; }
        public int ProductId { get; set; }

        // Navigation Properties
        public Wishlist Wishlist { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}