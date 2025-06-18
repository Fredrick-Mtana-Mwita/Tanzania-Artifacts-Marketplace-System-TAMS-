using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomePageViewModel
            {
                FeaturedProducts = await _productRepository.GetFeaturedAsync(),
                NewArrivals = await _productRepository.GetNewArrivalsAsync(),
                BestSellers = await _productRepository.GetBestSellersAsync()
            };
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
