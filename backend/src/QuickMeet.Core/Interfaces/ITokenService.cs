namespace QuickMeet.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(int providerId, string email);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token, out int providerId);
}
