using BlitzMall_Backend.DTOs.Auth;

namespace BlitzMall_Backend.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<AuthResponseDto> GoogleLoginAsync(
          string idToken);
        Task ResetPasswordAsync(ResetPasswordDto dto);

        Task ChangePasswordAsync(
            int userId,
            ChangePasswordDto dto);


    }
}
