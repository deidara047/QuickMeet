using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

public interface IQuickMeetDbContext
{
    DbSet<Provider> Providers { get; }
    DbSet<EmailVerificationToken> EmailVerificationTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
