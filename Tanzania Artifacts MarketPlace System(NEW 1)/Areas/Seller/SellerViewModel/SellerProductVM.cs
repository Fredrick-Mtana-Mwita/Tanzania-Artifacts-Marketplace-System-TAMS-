using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel
{
    public class SellerProductVM
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        public string ProductHistory { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        public bool IsFeatured { get; set; } = false;

        public IFormFile? Image { get; set; }

        public string? ExistingImagePath { get; set; }
    }

}


