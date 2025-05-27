using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("CartItem")]
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = default!;
        public int Quantity { get; set; }
        public int CartId { get; set; }
        public virtual Cart Cart { get; set; } = default!;
    }
}
