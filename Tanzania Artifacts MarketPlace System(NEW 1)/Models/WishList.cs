using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("WishList")]
    public class Wishlist
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = default!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "Default Wishlist";

        public bool IsPublic { get; set; } = true;

        public virtual ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
