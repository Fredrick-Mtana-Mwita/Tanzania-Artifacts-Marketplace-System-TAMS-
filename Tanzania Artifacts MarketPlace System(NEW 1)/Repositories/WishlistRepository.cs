namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly ApplicationDbContext _context;


        public WishlistRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Wishlist> GetOrCreateWishlistAsync(string userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return wishlist;
        }

        public async Task AddToWishlistAsync(string userId, int productId)
        {
            var wishlist = await GetOrCreateWishlistAsync(userId);

            bool exists = wishlist.Items.Any(i => i.ProductId == productId);
            if (!exists)
            {
                wishlist.Items.Add(new WishlistItem
                {
                    ProductId = productId,
                    WishlistId = wishlist.Id
                });

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(string userId, int productId)
        {
            var wishlist = await GetOrCreateWishlistAsync(userId);

            var item = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<WishlistItem>> GetWishlistItemsAsync(string userId)
        {
            var wishlist = await GetOrCreateWishlistAsync(userId);
            return wishlist.Items.ToList();
        }
    }
}
