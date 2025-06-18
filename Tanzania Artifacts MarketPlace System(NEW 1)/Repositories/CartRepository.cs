using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
               .Include(c => c.Items)
               .ThenInclude(i => i.Product)
               .FirstOrDefaultAsync(c => c.UserId == userId)
               ?? new Cart { UserId = userId };
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetCartByUserIdAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = quantity });
            }
            if (cart.Id == 0)
                _context.Carts.Add(cart);

            await SaveAsync();
        }

        public async Task DecreaseQuantityAsync(string userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                    {
                        cart.Items.Remove(item);
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await SaveAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            cart.Items.Clear();
            await SaveAsync();
        }

        public async Task SaveAsync() => await _context.SaveChangesAsync();

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.Items == null)
                return 0;

            return cart.Items.Sum(i => i.Quantity);
        }
    }
}
