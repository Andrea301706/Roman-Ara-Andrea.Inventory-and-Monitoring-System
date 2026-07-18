using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Roman_Ara_Andrea.Inventory_and_Monitoring_System.Helpers;

public class InviteTokenService
{
    // Secret key used to sign the JWT token.
    // For a real application, store this in configuration instead of hardcoding it.
    private const string SecretKey = "ThisIsMyVerySecretKey123456789123456";

    public string CreateInviteToken(int userId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", userId.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateInviteToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(SecretKey);

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                },
                out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}