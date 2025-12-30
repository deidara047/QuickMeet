using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.Infrastructure.Data;

public class QuickMeetDbContext : DbContext, IQuickMeetDbContext
{
    public QuickMeetDbContext(DbContextOptions<QuickMeetDbContext> options) : base(options)
    {
    }

    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProvider(modelBuilder);
        ConfigureEmailVerificationToken(modelBuilder);
    }

    private static void ConfigureProvider(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Provider>();

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.PhotoUrl)
            .HasMaxLength(2048);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(p => p.Status)
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(p => p.EmailVerifiedAt)
            .HasColumnType("datetimeoffset");

        builder.HasIndex(p => p.Email)
            .IsUnique()
            .HasDatabaseName("IX_Provider_Email_Unique");

        builder.HasIndex(p => p.Username)
            .IsUnique()
            .HasDatabaseName("IX_Provider_Username_Unique");

        builder.HasMany(p => p.EmailVerificationTokens)
            .WithOne(t => t.Provider)
            .HasForeignKey(t => t.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureEmailVerificationToken(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<EmailVerificationToken>();

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.CreatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(t => t.ExpiresAt)
            .HasColumnType("datetimeoffset");

        builder.Property(t => t.UsedAt)
            .HasColumnType("datetimeoffset");

        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("IX_EmailVerificationToken_Token_Unique");

        builder.HasIndex(t => new { t.ProviderId, t.IsUsed })
            .HasDatabaseName("IX_EmailVerificationToken_ProviderId_IsUsed");
    }
}
