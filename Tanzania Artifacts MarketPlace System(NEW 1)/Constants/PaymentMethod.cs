using System.ComponentModel.DataAnnotations;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Constants
{
    public enum PaymentMethod
    {
        [Display(Name = "Cash on Delivery")]
        CashOnDelivery,

        [Display(Name = "Mobile Money")]
        MobileMoney,

        [Display(Name = "PayPal")]
        Paypal
    }
}
