using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Marathon.Models;

namespace Marathon.Services
{
    public class JwtServives
    {
        private readonly IConfiguration _config;

        public JwtServives(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UserAccount user)
        {
            
            var secret = _config["JwtSettings:Secret"];

            if (string.IsNullOrEmpty(secret))
                throw new Exception("JWT Secret is missing in appsettings.json!");

            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

           
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserAccountId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            
            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpireMinutes"] ?? "120")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}