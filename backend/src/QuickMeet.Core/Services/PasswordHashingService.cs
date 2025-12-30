using BC = BCrypt.Net.BCrypt;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.Core.Services;

public class PasswordHashingService : IPasswordHashingService
{
    public string HashPassword(string password)
    {
        return BC.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
        {
            return false;
        }
        
        try
        {
            return BC.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
