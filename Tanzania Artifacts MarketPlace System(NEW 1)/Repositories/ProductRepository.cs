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
            => await _context.Products.ToListAsync ();

        // Retrieves a single product by its ID
        public async Task<Product?> GetByIdAsync(int id)
            => await _context.Products.FirstOrDefaultAsync (p=>p.Id == id);

        // Searches products where the name or description contains the keyword
        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
                    => await _context.Products
                    .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                    .ToListAsync();
    }
}
