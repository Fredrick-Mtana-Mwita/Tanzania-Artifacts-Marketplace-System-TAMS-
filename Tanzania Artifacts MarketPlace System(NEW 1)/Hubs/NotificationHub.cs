using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity != null && user.Identity.IsAuthenticated)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value;

                if (role == "Admin")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
                else if (role == "Seller")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Sellers");
                }
                else if (role == "User")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Users");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

