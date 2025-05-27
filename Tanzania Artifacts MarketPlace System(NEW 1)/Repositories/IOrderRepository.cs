namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Repositories
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);
        Task<Order?> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(string userId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task UpdateAsync(Order order);
    }
}
