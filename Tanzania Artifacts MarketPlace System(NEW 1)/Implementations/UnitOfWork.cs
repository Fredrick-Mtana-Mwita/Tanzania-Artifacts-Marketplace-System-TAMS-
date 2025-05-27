using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Data;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
      private readonly ApplicationDbContext _context;
        
        // Repository for product data 
        public IProductRepository Products { get; }

        public UnitOfWork(ApplicationDbContext context, IProductRepository productRepository)
        {
            _context = context;
            Products = productRepository;
        }

        // Saves changes to the database 
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
