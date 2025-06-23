using Microsoft.AspNetCore.SignalR;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Hubs;
using Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Services
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hub;
        public NotificationSender(IHubContext<NotificationHub> hub)
        {
            _hub = hub;
        }

        public async Task SendToAdmin(string title, string message)
        {
            await _hub.Clients.Group("Admins").SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendToUser(string userId, string title, string message)
        {
            await _hub.Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }

    }
}
