namespace QuickMeet.Core.Entities;

public class EmailVerificationToken
{
    public int Id { get; set; }
    
    public int ProviderId { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public bool IsUsed { get; set; } = false;
    
    public DateTimeOffset? UsedAt { get; set; }
    
    public Provider Provider { get; set; } = null!;
}
