using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Interfaces;

namespace TechZone.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CartDto?> GetUserCartAsync(string userId)
        {
            var carts = await _unitOfWork.Repository<Cart>()
                .FindAsync(c => c.UserId == userId);

            var cart = carts.FirstOrDefault();
            if (cart == null) return null;

            var cartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartId == cart.Id);

            var cartItemDtos = new List<CartItemDto>();
            foreach (var item in cartItems)
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    cartItemDtos.Add(new CartItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductImage = product.ImageUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        StockQuantity = product.StockQuantity
                    });
                }
            }

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cartItemDtos
            };
        }

        public async Task<CartDto> AddToCartAsync(AddToCartDto dto)
        {
            // Get or create cart
            var carts = await _unitOfWork.Repository<Cart>()
                .FindAsync(c => c.UserId == dto.UserId);

            var cart = carts.FirstOrDefault();
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = dto.UserId,
                    LastActivityDate = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Cart>().AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            // Check if product exists
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new Exception("Product not found");

            // Check if item already in cart
            var existingItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId);

            var existingItem = existingItems.FirstOrDefault();

            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += dto.Quantity;
                await _unitOfWork.Repository<CartItem>().UpdateAsync(existingItem);
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    UnitPrice = product.DiscountPrice ?? product.Price
                };
                await _unitOfWork.Repository<CartItem>().AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetUserCartAsync(dto.UserId) ?? new CartDto();
        }

        public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemDto dto)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(dto.CartItemId);
            if (cartItem == null)
                throw new Exception("Cart item not found");

            cartItem.Quantity = dto.Quantity;
            await _unitOfWork.Repository<CartItem>().UpdateAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();

            var cart = await _unitOfWork.Repository<Cart>().GetByIdAsync(cartItem.CartId);
            return await GetUserCartAsync(cart?.UserId ?? "") ?? new CartDto();
        }

        public async Task<bool> RemoveFromCartAsync(int cartItemId)
        {
            var cartItem = await _unitOfWork.Repository<CartItem>().GetByIdAsync(cartItemId);
            if (cartItem == null) return false;

            await _unitOfWork.Repository<CartItem>().DeleteAsync(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var carts = await _unitOfWork.Repository<Cart>()
                .FindAsync(c => c.UserId == userId);

            var cart = carts.FirstOrDefault();
            if (cart == null) return false;

            var cartItems = await _unitOfWork.Repository<CartItem>()
                .FindAsync(ci => ci.CartId == cart.Id);

            foreach (var item in cartItems)
            {
                await _unitOfWork.Repository<CartItem>().DeleteAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}