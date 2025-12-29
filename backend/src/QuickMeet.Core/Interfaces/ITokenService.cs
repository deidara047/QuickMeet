namespace QuickMeet.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid providerId, string email);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token, out Guid providerId);
}
