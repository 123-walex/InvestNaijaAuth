using InvestNaijaAuth.Entities;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace InvestNaijaAuth.Servicies
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        RefreshTokens CreateRefreshToken(string ipAddress);
    }
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreateAccessToken(User user)
        {

            if (user.Id == default)
                throw new ArgumentException("User ID is invalid", nameof(user.Id));

            if (user == null)
                throw new ArgumentException("The User is empty from the database", nameof(user));

            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required", nameof(user.Username));

            if (string.IsNullOrWhiteSpace(user.EmailAddress))
                throw new ArgumentException("Email is required", nameof(user.EmailAddress));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.EmailAddress),
                new Claim(ClaimTypes.Role, user.Role)

            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<String>("JwtSettings:AccessToken")!))
                              ?? throw new InvalidOperationException("JWT Accesstoken key is not configured.");

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration.GetValue<String>("JwtSettings:Issuer")
                             ?? throw new InvalidOperationException("JWT issuer is not configured."),
                audience: _configuration.GetValue<String>("JwtSettings:Audience")
                             ?? throw new InvalidOperationException("JWT audience is not configured."),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
        public RefreshTokens CreateRefreshToken(string ipAddress)
        {
            return new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }
    }
}

