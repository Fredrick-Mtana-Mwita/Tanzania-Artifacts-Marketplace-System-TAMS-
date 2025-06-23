using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Constants;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("Order")]
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string? SellerId { get; set; }
        public virtual ApplicationUser User { get; set; } = default!;

        public DateTime? ShippedDate { get; set; }

        public string ShippingCity { get; set; } = string.Empty;

        public OrderStatus Status { get; set; } = OrderStatus.Ordered;

        public string TrackingNumber { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

        public string TransactionId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredDate { get; set; }

        public bool IsCancelled { get; set; } = false;

        public DateTime? CancelledDate { get; set; }

        public decimal? RefundAmount { get; set; }


        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        [NotMapped]
        public decimal CalculatedTotalAmount => Items.Sum(item => item.UnitPrice * item.Quantity);
    }
}
