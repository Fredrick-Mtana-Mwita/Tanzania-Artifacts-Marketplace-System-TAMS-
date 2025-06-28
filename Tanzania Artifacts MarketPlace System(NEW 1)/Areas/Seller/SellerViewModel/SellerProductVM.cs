using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel
{
    public class SellerProductVM
    {
        public int ProductId { get; set; }

        [Required]
        public string? ProductName { get; set; }
        public int StockQuantity { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; }
        public string? ExistingImagePath { get; set; }
    }

}


