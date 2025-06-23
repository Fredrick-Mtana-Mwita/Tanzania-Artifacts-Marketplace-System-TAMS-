using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.SignalR
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
