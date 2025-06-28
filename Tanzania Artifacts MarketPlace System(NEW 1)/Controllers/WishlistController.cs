using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;

        public WishlistController(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _wishlistRepository.GetWishlistItemsAsync(userId!);
            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _wishlistRepository.AddToWishlistAsync(userId!, productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _wishlistRepository.RemoveFromWishlistAsync(userId!, productId);
            return RedirectToAction("Index");
        }
    }
}
