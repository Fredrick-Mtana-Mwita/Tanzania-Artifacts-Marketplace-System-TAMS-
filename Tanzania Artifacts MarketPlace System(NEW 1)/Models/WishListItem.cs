using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    public class WishListItem
    {
        [Table("WishListItem")]
        public class WishlistItem
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public virtual Product Product { get; set; } = default!;
            public int WishlistId { get; set; }
            public virtual Wishlist Wishlist { get; set; } = default!;
        }
    }
}
