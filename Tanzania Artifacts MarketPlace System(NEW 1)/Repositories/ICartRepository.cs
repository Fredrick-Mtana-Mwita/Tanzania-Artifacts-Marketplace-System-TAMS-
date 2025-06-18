namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public interface ICartRepository
    {
        Task AddToCartAsync(string userId, int productId, int quantity);
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task RemoveFromCartAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
        Task DecreaseQuantityAsync(string userId, int productId);
        Task<int> GetCartItemCountAsync(string userId);
        Task SaveAsync();
    }
}
