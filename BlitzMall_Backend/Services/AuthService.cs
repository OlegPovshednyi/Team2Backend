using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Auth;
using BlitzMall_Backend.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace BlitzMall_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(
            AppDbContext db,
            IConfiguration config,
            IEmailService emailService,
            IMemoryCache cache)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already in use.");

            var buyerRole = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name == "Buyer")
                ?? throw new InvalidOperationException("Default role not found.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = buyerRole.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Active"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user, buyerRole.Name!);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return BuildResponse(user, user.Role!.Name!);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

           
            if (user == null)
                return;

            var code = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var cacheKey = $"password-reset:{dto.Email}";

            _cache.Set(
                cacheKey,
                code,
                TimeSpan.FromMinutes(10));

            await _emailService.SendPasswordResetCodeAsync(
                dto.Email,
                code);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var cacheKey = $"password-reset:{dto.Email}";

            if (!_cache.TryGetValue(
                    cacheKey,
                    out string? savedCode))
            {
                throw new InvalidOperationException(
                    "Invalid or expired reset code.");
            }

            if (savedCode != dto.Code)
            {
                throw new InvalidOperationException(
                    "Invalid or expired reset code.");
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Invalid or expired reset code.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

           
            _cache.Remove(cacheKey);

            await _db.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(
            int userId,
            ChangePasswordDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(
                    dto.CurrentPassword,
                    user.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _db.SaveChangesAsync();
        }

        private AuthResponseDto BuildResponse(
            User user,
            string role)
        {
            return new AuthResponseDto
            {
                Token = GenerateToken(user, role),
                Email = user.Email,
                Name = user.Name ?? string.Empty,
                Role = role
            };
        }

        private string GenerateToken(
            User user,
            string role)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]!));

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    role),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(
                        _config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }


        public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
        {
            var clientId = _config["Google:ClientId"]
                ?? throw new InvalidOperationException("Google Client ID is not configured.");

            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

            if (payload.EmailVerified != true || string.IsNullOrEmpty(payload.Email))
                throw new UnauthorizedAccessException("Invalid Google account.");

            var user = await _db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                var role = await _db.Roles
                    .FirstOrDefaultAsync(r => r.Name == "Buyer")
                    ?? throw new InvalidOperationException("Default role not found.");

                user = new User
                {
                    Name = payload.Name ?? payload.Email,
                    Email = payload.Email,
                    PasswordHash = null,
                    RoleId = role.Id,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    Role = role
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }

            return BuildResponse(user, user.Role!.Name!);
        }

    }
    }

