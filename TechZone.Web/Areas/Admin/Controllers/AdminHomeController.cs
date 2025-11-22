using Microsoft.AspNetCore.Mvc;
[Area("Admin")]                                 // ← ही लाईन आत्ताच टाक!!
[Route("admin/[controller]/[action]/{id?}")]
public class AdminHomeController : Controller
{
    /*private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly ICategoryService _categoryService;

    public HomeController(
        IProductService productService,
        IOrderService orderService,
        ICategoryService categoryService)
    {
        _productService = productService;
        _orderService = orderService;
        _categoryService = categoryService;
    }

    तात्पुरता parameterless constructor
    public AdminHomeController()
    {
    }*/

    public async Task<IActionResult> Index()
    {
       // सगळे services comment कर(फक्त dashboard साठी)
        ViewBag.TotalProducts = 999;     // fake data
        ViewBag.TotalCategories = 50;
        ViewBag.TotalOrders = 1250;
        ViewBag.PendingOrders = 18;
        ViewBag.TotalRevenue = 985000.00m;
        ViewBag.RecentOrders = new List<object>(); // empty list

        return View();
    }
}