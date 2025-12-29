namespace QuickMeet.Core.Entities;

public class EmailVerificationToken
{
    public Guid Id { get; set; }
    
    public Guid ProviderId { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsUsed { get; set; } = false;
    
    public DateTime? UsedAt { get; set; }
    
    public Provider Provider { get; set; } = null!;
}
