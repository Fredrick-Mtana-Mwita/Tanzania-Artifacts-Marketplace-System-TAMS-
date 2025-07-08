using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("Payment")]
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = default!;
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? PayPalPaymentId { get; set; }
        public string? PayerEmail { get; set; }
        public string Status { get; set; } = "Success"; // Success, Failed, Refunded
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}
