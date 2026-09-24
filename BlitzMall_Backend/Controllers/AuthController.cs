using BlitzMall_Backend.DTOs.Auth;
using BlitzMall_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlitzMall_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterAsync(dto);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(dto);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new
                {
                    message = "Invalid credentials."
                });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.ForgotPasswordAsync(dto);

                return Ok(new
                {
                    message =
                        "If the email exists, a reset code has been sent."
                });
            }
            catch
            {
                return Ok(new
                {
                    message =
                        "If the email exists, a reset code has been sent."
                });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _authService.ResetPasswordAsync(dto);

                return Ok(new
                {
                    message = "Password reset successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new
                    {
                        message = "Invalid user identity."
                    });
                }

                await _authService.ChangePasswordAsync(
                    userId,
                    dto);

                return Ok(new
                {
                    message = "Password changed successfully."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }
            [HttpPost("google")]
            public async Task<IActionResult> GoogleLogin(
                [FromBody] GoogleLoginDto dto)
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                try
                {
                    var result =
                        await _authService.GoogleLoginAsync(
                            dto.IdToken);

                    return Ok(result);
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new
                    {
                        message = ex.Message
                    });
                }
                catch (InvalidOperationException ex)
                {
                    return Conflict(new
                    {
                        message = ex.Message
                    });
                }
            }



        }
    }
