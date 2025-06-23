namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetUnreadByUserIdAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task<List<Notification>> GetAllByUserIdAsync(string userId);
        Task<List<Notification>> GetByUserIdAsync(string userId);
        Task<Notification?> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);

    }


}
