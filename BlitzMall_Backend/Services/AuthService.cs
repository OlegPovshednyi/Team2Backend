using BlitzMall_Backend.Data;
using BlitzMall_Backend.DTOs.Auth;
using BlitzMall_Backend.Models;
using FirebaseAdmin.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlitzMall_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        private static readonly Dictionary<string, string> _resetCodes = new();

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already in use.");

            var buyerRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Buyer")
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

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            return BuildResponse(user, user.Role!.Name!);
        }

        public async Task<AuthResponseDto> FirebaseLoginAsync(string idToken)
        {
            FirebaseToken decodedToken;
            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            }
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Invalid Firebase token.");
            }

            var email = decodedToken.Claims.TryGetValue("email", out var emailClaim)
                ? emailClaim?.ToString() ?? string.Empty
                : string.Empty;

            var phoneNumber = decodedToken.Claims.TryGetValue("phone_number", out var phoneClaim)
                ? phoneClaim?.ToString() ?? string.Empty
                : string.Empty;

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
                throw new UnauthorizedAccessException("Firebase token has no email or phone.");

            var name = decodedToken.Claims.TryGetValue("name", out var nameClaim)
                ? nameClaim?.ToString()
                : null;

            var buyerRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Buyer")
                ?? throw new InvalidOperationException("Default role not found.");

            User? user = null;

            if (!string.IsNullOrWhiteSpace(email))
            {
                user = await _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email);
            }

            if (user == null && !string.IsNullOrWhiteSpace(phoneNumber))
            {
                user = await _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Phone == phoneNumber);
            }

            if (user == null)
            {
                var placeholderEmail = !string.IsNullOrWhiteSpace(email)
                    ? email
                    : $"phone_{phoneNumber.TrimStart('+')}@blitzmall.phone";

                user = new User
                {
                    Name = name ?? (!string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : phoneNumber),
                    Email = placeholderEmail,
                    Phone = !string.IsNullOrWhiteSpace(phoneNumber) ? phoneNumber : null,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    RoleId = buyerRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Active"
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                user.Role = buyerRole;
            }
            else if (!string.IsNullOrWhiteSpace(phoneNumber) && string.IsNullOrWhiteSpace(user.Phone))
            {
                user.Phone = phoneNumber;
                await _db.SaveChangesAsync();
            }

            return BuildResponse(user, user.Role!.Name!);
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new InvalidOperationException("User not found.");

            _resetCodes[email] = "123456";

            await Task.CompletedTask;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            await Task.CompletedTask;

            if (_resetCodes.TryGetValue(email, out var stored))
                return stored == code;

            return false;
        }

        public async Task ResetPasswordAsync(string email, string code, string newPassword)
        {
            var valid = await VerifyCodeAsync(email, code);
            if (!valid)
                throw new InvalidOperationException("Invalid or expired code.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new InvalidOperationException("User not found.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _resetCodes.Remove(email);

            await _db.SaveChangesAsync();
        }

        private AuthResponseDto BuildResponse(User user, string role) => new()
        {
            Token = GenerateToken(user, role),
            Email = user.Email,
            Name = user.Name ?? string.Empty,
            Role = role
        };

        private string GenerateToken(User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
