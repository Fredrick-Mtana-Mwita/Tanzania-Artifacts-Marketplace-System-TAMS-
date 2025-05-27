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
    }
}
