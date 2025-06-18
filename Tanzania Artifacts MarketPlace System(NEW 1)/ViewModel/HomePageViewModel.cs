using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.ViewModel
{
    public class HomePageViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
        public IEnumerable<Product> NewArrivals { get; set; } = new List<Product>();
        public IEnumerable<Product> BestSellers { get; set; } = new List<Product>();
        public IEnumerable<Product> TrendingProducts { get; set; } = new List<Product>(); 
    }
}
