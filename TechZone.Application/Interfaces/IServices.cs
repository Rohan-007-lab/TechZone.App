using TechZone.Application.DTOs;
using TechZone.Domain.Enums;

namespace TechZone.Application.Interfaces
{
    /// <summary>
    /// Product Service Interface
    /// </summary>
    public interface IProductService
    {
        Task<PagedResponse<ProductListDto>> GetProductsAsync(ProductFilterDto filter);
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(CreateProductDto dto);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<List<ProductListDto>> GetFeaturedProductsAsync(int count = 10);
        Task<List<ProductListDto>> GetProductsByCategoryAsync(int categoryId);
    }

    /// <summary>
    /// Order Service Interface
    /// </summary>
    public interface IOrderService
    {
       Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<List<OrderDto>> GetUserOrdersAsync(string userId);
        Task<PagedResponse<OrderDto>> GetAllOrdersAsync(int pageNumber, int pageSize);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
        Task<bool> CancelOrderAsync(int orderId);
    }

    /// <summary>
    /// Cart Service Interface
    /// </summary>
    public interface ICartService
    {
        Task<CartDto?> GetUserCartAsync(string userId);
        Task<CartDto> AddToCartAsync(AddToCartDto dto);
        Task<CartDto> UpdateCartItemAsync(UpdateCartItemDto dto);
        Task<bool> RemoveFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(string userId);
        Task<IEnumerable<object>> GetCartItemsAsync(string userId);
    }

    /// <summary>
    /// Category Service Interface
    /// </summary>
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(CategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
    }

    /// <summary>
    /// Review Service Interface
    /// </summary>
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetProductReviewsAsync(int productId);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto);
        Task<bool> DeleteReviewAsync(int id);
        Task<bool> ApproveReviewAsync(int id);
    }

    /// <summary>
    /// Authentication Service Interface
    /// </summary>
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginDto dto);
        Task<ApiResponse<UserDto>> RegisterAsync(RegisterDto dto);
        Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordDto dto);
        Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(string userId);
    }

    /// <summary>
    /// Payment Service Interface
    /// </summary>
    public interface IPaymentService
    {
        Task<PaymentDto> ProcessPaymentAsync(int orderId, PaymentMethod method);
        Task<PaymentDto?> GetPaymentByOrderIdAsync(int orderId);
        Task<bool> RefundPaymentAsync(int paymentId);
    }
}