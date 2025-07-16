using Microsoft.EntityFrameworkCore;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
     => await _context.Products
         .Include(p => p.Images)
         .Where(p => p.IsApproved)
         .OrderByDescending(p => p.DateCreated)
         .ToListAsync();
        public async Task<Product?> GetByIdAsync(int id)
            => await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
            => await _context.Products
                .Include(p => p.Images)
                .Where(p => p.Name.Contains(keyword) || p.Description.Contains(keyword))
                .Where(p => p.IsApproved)
                .ToListAsync();

        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            return await _context.Products
                .Where(p => p.IsFeatured && p.IsApproved)
                .Include(p => p.Images) 
                .OrderByDescending(p => p.DateCreated)
                .Take(6)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetNewArrivalsAsync()
        {
            return await _context.Products
                .Include(p => p.Images) 
                .OrderByDescending(p => p.DateCreated)
                .Take(6)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetBestSellersAsync()
        {
            return await _context.Products
                .Include(p => p.Images) 
                .OrderByDescending(p => p.SoldCount)
                .Take(6)
                .ToListAsync();
        }

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

        public async Task CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
    }
}
