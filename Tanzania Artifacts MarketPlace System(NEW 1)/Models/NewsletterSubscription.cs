using System;
using System.ComponentModel.DataAnnotations;


namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    public class NewsletterSubscription
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
