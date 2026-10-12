using BlitzMall_Backend.DTOs.Auth;

namespace BlitzMall_Backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        Task<AuthResponseDto> GoogleLoginAsync(string idToken);

        Task<AuthResponseDto> FirebaseLoginAsync(string idToken);

        Task ForgotPasswordAsync(ForgotPasswordDto dto);

        Task<bool> VerifyCodeAsync(string email, string code);

        Task ResetPasswordAsync(ResetPasswordDto dto);

        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}