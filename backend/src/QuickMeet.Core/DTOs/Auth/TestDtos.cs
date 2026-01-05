namespace QuickMeet.Core.DTOs.Auth;

public class SeedUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? FullName { get; set; }
    public string? Password { get; set; }
}
