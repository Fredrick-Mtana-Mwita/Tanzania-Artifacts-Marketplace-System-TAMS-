using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly ICartRepository _cartRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartCountViewComponent(ICartRepository cartRepository, UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int count = 0;

            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
                if (user != null)
                {
                    count = await _cartRepository.GetCartItemCountAsync(user.Id);
                }
            }

            return View(count);
        }
    }
}
