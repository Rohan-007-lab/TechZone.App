using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;
using TechZone.Domain.Entities;
using TechZone.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace TechZone.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<List<ReviewDto>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ProductId == productId && r.IsApproved);

            var reviewDtos = new List<ReviewDto>();
            foreach (var review in reviews.OrderByDescending(r => r.CreatedAt))
            {
                var user = await _userManager.FindByIdAsync(review.UserId);
                reviewDtos.Add(new ReviewDto
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    UserId = review.UserId,
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Anonymous",
                    Rating = review.Rating,
                    Title = review.Title,
                    Comment = review.Comment,
                    IsVerifiedPurchase = review.IsVerifiedPurchase,
                    CreatedAt = review.CreatedAt
                });
            }

            return reviewDtos;
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto)
        {
            // Validate rating
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new Exception("Rating must be between 1 and 5");

            // Check if product exists
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(dto.ProductId);
            if (product == null)
                throw new Exception("Product not found");

            // Check if user already reviewed this product
            var existingReviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ProductId == dto.ProductId && r.UserId == dto.UserId);

            if (existingReviews.Any())
                throw new Exception("You have already reviewed this product");

            // Check if user purchased this product
            var orders = await _unitOfWork.Repository<Order>()
                .FindAsync(o => o.UserId == dto.UserId);

            var hasPurchased = false;
            foreach (var order in orders)
            {
                var orderItems = await _unitOfWork.Repository<OrderItem>()
                    .FindAsync(oi => oi.OrderId == order.Id && oi.ProductId == dto.ProductId);
                if (orderItems.Any())
                {
                    hasPurchased = true;
                    break;
                }
            }

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                Rating = dto.Rating,
                Title = dto.Title,
                Comment = dto.Comment,
                IsVerifiedPurchase = hasPurchased,
                IsApproved = false // Requires admin approval
            };

            await _unitOfWork.Repository<Review>().AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(dto.ProductId);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Anonymous",
                Rating = review.Rating,
                Title = review.Title,
                Comment = review.Comment,
                IsVerifiedPurchase = review.IsVerifiedPurchase,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(id);
            if (review == null) return false;

            var productId = review.ProductId;

            await _unitOfWork.Repository<Review>().DeleteAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(productId);

            return true;
        }

        public async Task<bool> ApproveReviewAsync(int id)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(id);
            if (review == null) return false;

            review.IsApproved = true;
            await _unitOfWork.Repository<Review>().UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(review.ProductId);

            return true;
        }

        private async Task UpdateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null) return;

            var reviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ProductId == productId && r.IsApproved);

            var reviewList = reviews.ToList();
            if (reviewList.Any())
            {
                product.AverageRating = (decimal)reviewList.Average(r => r.Rating);
                product.ReviewCount = reviewList.Count;
            }
            else
            {
                product.AverageRating = 0;
                product.ReviewCount = 0;
            }

            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}