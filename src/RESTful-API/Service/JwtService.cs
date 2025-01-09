using Microsoft.IdentityModel.Tokens;
using RESTful_API.Jwt;
using RESTful_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RESTful_API.Service
{
    /// <summary>
    /// JWT Token Service
    /// </summary>
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        // DI
        public JwtService(IConfiguration configuration)
        {
            _jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        }

        /// <summary>
        /// Generates the JWT token 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateToken(UserModel user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            try
            {
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
                    SigningCredentials = credentials,
                    Issuer = _jwtSettings.Issuer,
                    Audience = _jwtSettings.Audience,
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);

                return tokenHandler.WriteToken(securityToken);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating token", ex);

            }
        }   
    }
}
