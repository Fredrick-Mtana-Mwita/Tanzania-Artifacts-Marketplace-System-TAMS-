using Microsoft.AspNetCore.Mvc;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Implementations;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class ProductController : Controller
    {
        // Injected unit of work
        private readonly IUnitOfWork _unitOfWork;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Products
        //public async Task<IActionResult> Index()
        //{
        //    // Fetch all products
        //    var products = await _unitOfWork.Products.GetAllAsync();

        //    // Render Index view with product list
        //    return View(products);
        //}

        public async Task<IActionResult> Index(string? searchTerm, int? productId, bool? inStock, decimal? minPrice, decimal? maxPrice)
        {
            var allProducts = await _unitOfWork.Products.GetAllAsync();

            // Apply search by name or description
            if (!string.IsNullOrEmpty(searchTerm))
            {
                allProducts = allProducts.Where(p =>
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by Product ID
            if (productId.HasValue)
            {
                allProducts = allProducts.Where(p => p.Id == productId.Value);
            }

            // Filter by stock
            if (inStock.HasValue)
            {
                allProducts = allProducts.Where(p => p.InStock == inStock.Value);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                allProducts = allProducts.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                allProducts = allProducts.Where(p => p.Price <= maxPrice.Value);
            }

            return View(allProducts);
        }


        // GET: /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET: /Products/Search?keyword=somevalue
        public  async Task<IActionResult> Search(string keyword)
        {
            var results = await _unitOfWork.Products.SearchAsync(keyword);
            return View("Index",results);
        }
    }
}
