using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("Product")]
    public class Product
    {
        public int Id { get; set; }
        public string CategoryId { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public string ProductHistory { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool InStock { get; set; }
        public int StockQuantity { get; set; }
        public bool IsFeatured { get; set; } = false;
        public int SoldCount { get; set; } = 0;
        public bool IsApproved { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateUpdated { get; set; }

        // Relationships
        [Required]
        public string? SellerId { get; set; }
        public virtual ApplicationUser Seller { get; set; } = default!;
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
     

    }
}
