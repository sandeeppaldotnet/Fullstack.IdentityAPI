using Fullstack.IdentityAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Fullstack.IdentityAPI.Services
{
    public class JwtTokenService
    {
        private readonly JwtSettings _settings;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(
        JwtSettings settings,
        UserManager<ApplicationUser> userManager)
        {
            _settings = settings;
            _userManager = userManager;
        }

        public async Task<(string Token, DateTime ExpiresAtUtc)>
            CreateAccessTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email ?? string.Empty),

            new(
                ClaimTypes.NameIdentifier,
                user.Id),

            new(
                ClaimTypes.Name,
                user.UserName ?? string.Empty),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            claims.Add(
                new Claim(
                    "Department",
                    user.Department ?? string.Empty));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Key));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var expiresAtUtc =
                DateTime.UtcNow.AddMinutes(
                    _settings.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            var accessToken =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            return (accessToken, expiresAtUtc);
        }


        public string CreateRefreshToken()
        {
            var randomBytes =
                RandomNumberGenerator.GetBytes(64);

            return WebEncoders.Base64UrlEncode(
                randomBytes);
        }
        public string HashRefreshToken(
    string refreshToken)
        {
            var bytes =
                Encoding.UTF8.GetBytes(
                    refreshToken);

            var hash =
                SHA256.HashData(bytes);

            return Convert.ToBase64String(hash);
        }
    }
}


