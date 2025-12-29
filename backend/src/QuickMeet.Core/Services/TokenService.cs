using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.Core.Services;

public class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly int _accessTokenExpirationMinutes;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        
        _accessTokenExpirationMinutes = int.Parse(
            configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
    }

    public string GenerateAccessToken(Guid providerId, string email)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, providerId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("sub", providerId.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "QuickMeet",
            audience: "QuickMeetClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateAccessToken(string token, out Guid providerId)
    {
        providerId = Guid.Empty;

        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = "QuickMeet",
                ValidateAudience = true,
                ValidAudience = "QuickMeetClients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var providerIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (providerIdClaim != null && Guid.TryParse(providerIdClaim.Value, out var id))
            {
                providerId = id;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
