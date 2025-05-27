using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
          _cartRepository = cartRepository;
            _userManager = userManager;
        }

        private async Task<string> GetUserIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id ?? throw new Exception("User not found!");
        }

        public async Task<IActionResult> Index()
        {
            var userId = await GetUserIdAsync();
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = await GetUserIdAsync();
            await _cartRepository.AddToCartAsync(userId, productId, quantity);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> Decrease(int productId)
        {
            var userId = await GetUserIdAsync();
            await _cartRepository.DecreaseQuantityAsync(userId, productId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int productId)
        {
            var userId = await GetUserIdAsync();
            await _cartRepository.RemoveFromCartAsync(userId,productId);
            return RedirectToAction("Index");
        }

       
    }
}
