using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Areas.Seller.SellerViewModel
{
    public class SellerProfileViewModel
    {
        [Required]
        [Display(Name = "Business Name")]
        [StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [Display(Name = "Tax ID")]
        [StringLength(50)]
        public string TaxId { get; set; } = string.Empty;

        [Display(Name = "Store Description")]
        [StringLength(1000)]
        public string StoreDescription { get; set; } = string.Empty;

        [Display(Name = "Bank Account Info")]
        [StringLength(100)]
        public string BankAccountInfo { get; set; } = string.Empty;

        [Display(Name = "Contact Email")]
        [EmailAddress]
        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Store Logo")]
        public IFormFile? StoreLogo { get; set; }

        [Display(Name = "Store Banner")]
        public IFormFile? StoreBanner { get; set; }

        // Optional for view display
        public string? ExistingStoreLogoPath { get; set; }
        public string? ExistingStoreBannerPath { get; set; }
    }

}
