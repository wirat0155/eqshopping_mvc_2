using eqshopping.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace eqshopping.Repositories
{
    public interface IJwtTokenService
    {
        string GenerateToken(string userId, string role);
        string RefreshToken(string oldToken);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _config;
        private readonly IClaimsHelper _claimsHelper;

        public JwtTokenService(IOptions<JwtSettings> jwtSettings, IConfiguration config, IClaimsHelper claimsHelper)
        {
            _jwtSettings = jwtSettings.Value;
            _config = config;
            _claimsHelper = claimsHelper;
        }

        public string GenerateToken(string userId, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role),
                new Claim("version", _config["Application:Version"])
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryInHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public string RefreshToken(string oldToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            try
            {
                // Validate the old token (ensure it's still valid)
                tokenHandler.ValidateToken(oldToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Disabling clock skew for immediate expiry validation
                }, out SecurityToken validatedToken);

                // Extract claims from the old token
                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(c => c.Type == "nameid").Value;
                var role = jwtToken.Claims.First(c => c.Type == "role").Value;

                // Generate and return a new token with the same userId and role
                return GenerateToken(userId, role);
            }
            catch (SecurityTokenExpiredException)
            {
                // Token has expired, handle accordingly (e.g., require re-authentication)
                throw new Exception("Token has expired. User needs to log in again.");
            }
            catch (Exception ex)
            {
                // Handle any other token validation failures
                throw new Exception($"Token validation failed: {ex.Message}");
            }
        }
    }

}
