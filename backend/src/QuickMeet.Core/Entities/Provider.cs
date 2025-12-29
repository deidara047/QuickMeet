namespace QuickMeet.Core.Entities;

public class Provider
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? PhotoUrl { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public ProviderStatus Status { get; set; } = ProviderStatus.PendingVerification;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? EmailVerifiedAt { get; set; }
    
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = [];
}

public enum ProviderStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}
