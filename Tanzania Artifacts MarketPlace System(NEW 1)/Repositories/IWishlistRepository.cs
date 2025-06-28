namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public interface IWishlistRepository
    {
        Task<Wishlist> GetOrCreateWishlistAsync(string userId);
        Task AddToWishlistAsync(string userId, int productId);
        Task RemoveFromWishlistAsync(string userId, int productId);
        Task<List<WishlistItem>> GetWishlistItemsAsync(string userId);
    }
}
