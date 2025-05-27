namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.ViewModel
{
    
    public class CheckoutVM
    {
        public string ShippingCity { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }


        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount => Items.Sum(c => c.Product.Price * c.Quantity);
    }

}
