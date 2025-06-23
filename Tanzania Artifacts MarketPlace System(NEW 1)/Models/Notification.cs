using System.ComponentModel.DataAnnotations.Schema;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Models
{
    [Table("Notification")]
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = default!;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // NEW: For easier filtering or system notifications (e.g. 'AdminOnly')
        public NotificationType Type { get; set; } = NotificationType.General;

        // NEW (optional): Allow URL redirection
        public string? RedirectUrl { get; set; }

        // NEW (optional): Severity label (for UI: Info, Warning, Error, Success)
        public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;
    }
}
