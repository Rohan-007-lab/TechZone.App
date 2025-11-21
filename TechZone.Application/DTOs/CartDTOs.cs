namespace TechZone.Application.DTOs
{
    /// <summary>
    /// Cart Response DTO
    /// </summary>
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal SubTotal => CartItems.Sum(x => x.TotalPrice);
        public int ItemCount => CartItems.Sum(x => x.Quantity);
    }

    /// <summary>
    /// Cart Item DTO
    /// </summary>
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Add to Cart Request DTO
    /// </summary>
    public class AddToCartDto
    {
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Update Cart Item DTO
    /// </summary>
    public class UpdateCartItemDto
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Remove from Cart DTO
    /// </summary>
    public class RemoveFromCartDto
    {
        public int CartItemId { get; set; }
    }
}