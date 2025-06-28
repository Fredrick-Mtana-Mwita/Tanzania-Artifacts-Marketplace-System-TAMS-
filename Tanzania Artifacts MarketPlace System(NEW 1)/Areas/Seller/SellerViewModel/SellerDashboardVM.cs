namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel
{
    public class SellerDashboardVM
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProductsSold { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<TopProductVM> TopProducts { get; set; } = new();

    }
}
