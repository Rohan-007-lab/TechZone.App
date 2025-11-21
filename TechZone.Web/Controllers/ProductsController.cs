using Microsoft.AspNetCore.Mvc;
using TechZone.Application.DTOs;
using TechZone.Application.Interfaces;

namespace TechZone.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber = 1)
        {
            var filter = new ProductFilterDto
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = pageNumber,
                PageSize = 12,
                Status = Domain.Enums.ProductStatus.Active
            };

            var products = await _productService.GetProductsAsync(filter);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CategoryId = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(products);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Products/Category/5
        public async Task<IActionResult> Category(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found";
                return RedirectToAction(nameof(Index));
            }

            var products = await _productService.GetProductsByCategoryAsync(id);
            ViewBag.Category = category;

            return View(products);
        }

        // GET: Products/Search
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return RedirectToAction(nameof(Index));

            var filter = new ProductFilterDto
            {
                SearchTerm = term,
                PageNumber = 1,
                PageSize = 20,
                Status = Domain.Enums.ProductStatus.Active
            };

            var products = await _productService.GetProductsAsync(filter);
            ViewBag.SearchTerm = term;

            return View("Index", products);
        }
    }
}