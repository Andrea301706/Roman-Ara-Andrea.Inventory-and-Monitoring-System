using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Services
{
    public class ResetPasswordTokenService
    {
        private readonly IConfiguration _configuration;

        public ResetPasswordTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateResetToken(int userId)
        {
            var secretKey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException(
                    "Jwt:SecretKey is missing from configuration.");
            }

            if (Encoding.UTF8.GetByteCount(secretKey) < 32)
            {
                throw new InvalidOperationException(
                    "Jwt:SecretKey must be at least 32 characters long.");
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", userId.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var secretKey = _configuration["Jwt:SecretKey"];

            if (string.IsNullOrWhiteSpace(secretKey))
                return null;

            try
            {
                var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey));

                var tokenHandler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,

                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                    return null;

                if (!jwtToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
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