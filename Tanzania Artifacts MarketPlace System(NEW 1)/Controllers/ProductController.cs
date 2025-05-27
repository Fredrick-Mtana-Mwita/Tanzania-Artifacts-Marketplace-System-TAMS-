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
        public async Task<IActionResult> Index()
        {
            // Fetch all products
            var products = await _unitOfWork.Products.GetAllAsync();

            // Render Index view with product list
            return View(products);
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
