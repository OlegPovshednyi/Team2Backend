using BlitzMall_Backend.DTOs.Auth;

namespace BlitzMall_Backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> FirebaseLoginAsync(string idToken);
        Task ForgotPasswordAsync(string email);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task ResetPasswordAsync(string email, string code, string newPassword);
    }
}
