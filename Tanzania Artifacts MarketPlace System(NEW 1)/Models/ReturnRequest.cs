using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("ReturnRequest")]
    public class ReturnRequest
    {
        public int Id { get; set; }
        public int OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; } = default!;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public string ApprovalNotes { get; set; } = string.Empty;
    }
}
