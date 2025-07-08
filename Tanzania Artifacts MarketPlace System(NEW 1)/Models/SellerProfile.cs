using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("SellerProfile")]
    public class SellerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = default!;

        [Required, StringLength(100)]
        public string BusinessName { get; set; } = string.Empty;

        [StringLength(50)]
        public string TaxId { get; set; } = string.Empty;

        [StringLength(1000)]
        public string StoreDescription { get; set; } = string.Empty;

        [StringLength(100)]
        public string BankAccountInfo { get; set; } = string.Empty;

        public bool IsApproved { get; set; } = false;

        // Optional future additions
        [StringLength(100)]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(255)]
        public string StoreLogoPath { get; set; } = string.Empty;

        [StringLength(255)]
        public string StoreBannerPath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
