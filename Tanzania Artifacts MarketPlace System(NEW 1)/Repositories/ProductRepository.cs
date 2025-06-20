using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor injection of the database context
        public ProductRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        // Retrieves all products asynchronously
        public async Task<IEnumerable<Product>> GetAllAsync()
            => await _context.Products.ToListAsync();

        // Retrieves a single product by its ID
        public async Task<Product?> GetByIdAsync(int id)
            => await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        // Searches products where the name or description contains the keyword
        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
            => await _context.Products
                            .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                            .ToListAsync();

        // Gets featured products (e.g., manually marked)
        public async Task<IEnumerable<Product>> GetFeaturedAsync()
            => await _context.Products
                             .Where(p => p.IsFeatured)
                             .OrderByDescending(p => p.DateCreated)
                             .Take(6)
                             .ToListAsync();

        // Gets the most recently added products
        public async Task<IEnumerable<Product>> GetNewArrivalsAsync()
            => await _context.Products
                             .OrderByDescending(p => p.DateCreated)
                             .Take(6)
                             .ToListAsync();

        // Gets best-selling products (based on SoldCount)
        public async Task<IEnumerable<Product>> GetBestSellersAsync()
            => await _context.Products
                             .OrderByDescending(p => p.SoldCount)
                             .Take(6)
                             .ToListAsync();
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

    }
}
