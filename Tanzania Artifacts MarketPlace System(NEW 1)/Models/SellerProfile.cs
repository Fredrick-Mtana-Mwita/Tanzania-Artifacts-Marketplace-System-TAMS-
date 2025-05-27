using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("SellerProfile")]
    public class SellerProfile
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = default!;
        public string BusinessName { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string StoreDescription { get; set; } = string.Empty;
        public string BankAccountInfo { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
    }
}
