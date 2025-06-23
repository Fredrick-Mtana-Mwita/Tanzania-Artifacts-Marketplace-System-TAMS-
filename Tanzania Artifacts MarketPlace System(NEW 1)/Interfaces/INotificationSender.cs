namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces
{
    public interface INotificationSender
    {
        Task SendToAdmin(string title, string message);
        Task SendToUser(string userId, string title, string message);
    }
}
