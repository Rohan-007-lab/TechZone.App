using TechZone.Domain.Common;

namespace TechZone.Domain.Entities
{
    /// <summary>
    /// Shopping Cart Entity
    /// </summary>
    public class Cart : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public DateTime LastActivityDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ApplicationUser? User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }

    /// <summary>
    /// Cart Item Entity
    /// </summary>
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation Properties
        public Cart Cart { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}