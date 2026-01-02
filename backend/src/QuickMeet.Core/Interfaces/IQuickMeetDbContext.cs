using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface IQuickMeetDbContext
{
    DbSet<Provider> Providers { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    DbSet<ProviderAvailability> ProviderAvailabilities { get; }
    DbSet<TimeSlot> TimeSlots { get; }
    DbSet<Break> Breaks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
