namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.ViewModel
{
    public class AdminDashboardVM
    {
        public int TotalUsers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }

        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
    }

}
