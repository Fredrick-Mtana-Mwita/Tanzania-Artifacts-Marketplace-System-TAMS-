namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Admin.ViewModel
{
    public class ReportViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalSellers { get; set; }

        public List<string> Months { get; set; } = new();
        public List<decimal> MonthlySales { get; set; } = new();
        public List<int> MonthlyOrders { get; set; } = new();
    }
}
