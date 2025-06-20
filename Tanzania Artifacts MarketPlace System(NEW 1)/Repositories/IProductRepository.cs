using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    // Interface for defining methods for accessing product data
    public interface IProductRepository
    {
        // Gets all products from the database
        Task<IEnumerable<Product>> GetAllAsync();

        // Gets a single product by its ID
        Task<Product?> GetByIdAsync(int id);

        // Searches for products matching the keyword in name or description
        Task<IEnumerable<Product>> SearchAsync(string keyword);

        // Gets featured products (e.g., manually marked)
        Task<IEnumerable<Product>> GetFeaturedAsync();

        // Gets the most recently added products
        Task<IEnumerable<Product>> GetNewArrivalsAsync();

        // Gets best-selling products (based on SoldCount)
        Task<IEnumerable<Product>> GetBestSellersAsync();
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);

    }
}
