using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Project.Core.Domain.Entities;
using Project.Core.DTO;
using Project.Core.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Project.Core.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public JwtService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<AuthenticationResponse> CreateJwtTokenAsync(User user, string clientType = "Web")
        {
            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT Key is missing in configuration.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            // 1. تحديد الوقت والجمهور بناءً على نوع العميل
            DateTime expiration;
            string audience;

            var type = clientType.ToLower();

            if (type == "mobile" || type == "android" || type == "ios")
            {
                expiration = DateTime.UtcNow.AddDays(90);
                audience = _configuration["Jwt:AudienceMobile"] ?? "HayyAppMobile";
            }
            else
            {
                double minutes = double.TryParse(_configuration["Jwt:EXPIRATION_MINUTES"], out var m) ? m : 60;
                expiration = DateTime.UtcNow.AddMinutes(minutes);
                audience = _configuration["Jwt:AudienceWeb"] ?? "HayyAppWeb";
            }

            // 2. تجهيز الـ Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName ?? "Unknown"),
                new Claim("ClientType", clientType)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. التشفير والتوقيع
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenObject = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            // 4. تجهيز الـ Refresh Token
            string refreshToken = GenerateRefreshToken();

            DateTime refreshTokenExpiration = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_MINUTES"] ?? "43200"));

            // 5. إرجاع الـ DTO
            return new AuthenticationResponse()
            {
                Token = token,
                RefreshToken = refreshToken,
                Expiration = expiration,
                RefreshTokenExpirationDateTime = refreshTokenExpiration,
                PersonName = user.FullName,
                Email = user.Email,
            };
        }

        private string GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<ClaimsPrincipal?> GetPrincipalFromJwtToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudiences = new[]
                {
                    _configuration["Jwt:AudienceWeb"],
                    _configuration["Jwt:AudienceMobile"]
                },
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),

                ValidateLifetime = false // مهم جداً: يسمح بقراءة التوكن المنتهي
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                // استخدام Task.Run لأن ValidateToken عملية متزامنة (Synchronous)
                // لكننا نحتاجها في دالة Async، هذا حل بسيط
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}