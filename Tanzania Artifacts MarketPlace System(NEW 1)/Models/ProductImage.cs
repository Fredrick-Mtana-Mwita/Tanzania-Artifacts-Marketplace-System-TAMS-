using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("ProductImage")]
    public class ProductImage
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? OriginalFileName { get; set; }

        public string? AltText { get; set; }

        public bool IsMain { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ProductId { get; set; }

        public virtual Product Product { get; set; } = default!;
    }

}
