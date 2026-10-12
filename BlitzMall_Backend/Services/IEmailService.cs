namespace BlitzMall_Backend.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string email, string code);
    }
}