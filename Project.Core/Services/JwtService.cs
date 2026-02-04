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
            var roles = await _userManager.GetRolesAsync(user);


            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT Key is missing in configuration/UserSecrets.");
            }

            DateTime expiration;
            string audience;

            // لو العميل موبايل -> وقت طويل + جمهور الموبايل
            if (clientType.ToLower() == "mobile" || clientType.ToLower() == "android" || clientType.ToLower() == "ios")
            {
                expiration = DateTime.UtcNow.AddDays(30); // شهر كامل
                audience = _configuration["Jwt:AudienceMobile"]!;
            }
            else // لو ويب -> وقت قصير + جمهور الويب
            {
                // بنجيب الدقائق من الكونفيج، ولو مش موجودة بنفترض 60 دقيقة
                double minutes = double.TryParse(_configuration["Jwt:EXPIRATION_MINUTES"], out var m) ? m : 60;
                expiration = DateTime.UtcNow.AddMinutes(minutes);
                audience = _configuration["Jwt:AudienceWeb"]!;
            }

            // 4️⃣ التعديل: تحسين الـ Claims
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.DateTime),
                
                // تصحيح: الـ NameIdentifier يفضل يكون الـ ID مش الإيميل (عشان ده الـ Primary Key)
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName!),
                
                // إضافة نوع العميل عشان لو حبيت تعرف التوكن ده طالع لمين
                new Claim("ClientType", clientType)
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: audience, // 👈 هنا بنستخدم الجمهور المتغير
                claims: claims,
                expires: expiration, // 👈 وهنا الوقت المتغير
                signingCredentials: signingCredentials
            );

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenGenerator);

            return new AuthenticationResponse()
            {
                Token = token,
                Email = user.Email,
                PersonName = user.FullName,
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpirationDateTime = DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["RefreshToken:EXPIRATION_MINUTES"]))

            };
        }
        private string GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }


        public async Task<ClaimsPrincipal?> GetPrincipalFromJwtToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                // هنا نسمح بالاثنين عشان التوكن ممكن يكون موبايل أو ويب
                ValidAudiences = new[]
        {
            _configuration["Jwt:AudienceWeb"],
            _configuration["Jwt:AudienceMobile"]
        },
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),

                ValidateLifetime = false //should be false
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            ClaimsPrincipal principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

    }
}

