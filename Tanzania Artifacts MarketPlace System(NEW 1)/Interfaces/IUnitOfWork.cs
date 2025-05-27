using System.Threading.Tasks;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces
{
    public interface IUnitOfWork
    {
        // Property for accessing product-related data
        IProductRepository Products { get; }

        // Saves all changes to the database
        Task SaveAsync();                    
    }
}


