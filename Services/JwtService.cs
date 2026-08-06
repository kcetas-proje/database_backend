using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KcetasAboneApi.Services
{
    /// <summary>
    /// Kimlik doğrulama işlemleri için JSON Web Token (JWT) üreten yardımcı servis.
    /// </summary>
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Kullanıcı bilgileri ve rol id'si verilerek, belirlenen şifre ve süre ile geçerli bir JWT (Bearer Token) döndürür.
        /// </summary>
        public string GenerateToken(string kullaniciAdi, short rolId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kullaniciAdi),
                new Claim(ClaimTypes.Role, rolId.ToString()) 
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["JwtSettings:ExpirationInMinutes"])),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}