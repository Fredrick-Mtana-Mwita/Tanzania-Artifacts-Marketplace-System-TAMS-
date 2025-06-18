using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Controllers
{
    public class BaseController : Controller
    {
        protected readonly ICartRepository _cartRepository;
        protected readonly UserManager<ApplicationUser> _userManager;

        public BaseController(ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
        }

        protected async Task LoadCartCountAsync()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var count = await _cartRepository.GetCartItemCountAsync(user.Id);
                    ViewBag.CartCount = count;
                }
            }
        }
    }
}
