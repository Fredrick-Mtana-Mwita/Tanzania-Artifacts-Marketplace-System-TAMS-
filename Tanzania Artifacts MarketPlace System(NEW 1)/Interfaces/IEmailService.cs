namespace Tanzania_Artifacts_MarketPlace_System_NEW_1_.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

}
