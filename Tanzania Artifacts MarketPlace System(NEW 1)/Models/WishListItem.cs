using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("WishListItem")]
    public class WishlistItem
    {
        public int Id { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;

        [ForeignKey("Wishlist")]
        public int WishlistId { get; set; }
        public virtual Wishlist Wishlist { get; set; } = default!;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}
