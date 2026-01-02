namespace QuickMeet.Core.Entities;

public class Provider
{
    public int Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public string? PhotoUrl { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public ProviderStatus Status { get; set; } = ProviderStatus.PendingVerification;
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? EmailVerifiedAt { get; set; }
    
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = [];
    
    public ICollection<ProviderAvailability> ProviderAvailabilities { get; set; } = [];
    
    public ICollection<TimeSlot> TimeSlots { get; set; } = [];
}

public enum ProviderStatus
{
    PendingVerification = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}
