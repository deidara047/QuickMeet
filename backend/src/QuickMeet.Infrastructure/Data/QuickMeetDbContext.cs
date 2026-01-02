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
    public DbSet<ProviderAvailability> ProviderAvailabilities { get; set; } = null!;
    public DbSet<TimeSlot> TimeSlots { get; set; } = null!;
    public DbSet<Break> Breaks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProvider(modelBuilder);
        ConfigureEmailVerificationToken(modelBuilder);
        ConfigureProviderAvailability(modelBuilder);
        ConfigureTimeSlot(modelBuilder);
        ConfigureBreak(modelBuilder);
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

    private static void ConfigureProviderAvailability(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ProviderAvailability>();

        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.DayOfWeek)
            .HasConversion<int>();

        builder.Property(pa => pa.CreatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(pa => pa.UpdatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(pa => new { pa.ProviderId, pa.DayOfWeek })
            .IsUnique()
            .HasDatabaseName("IX_ProviderAvailability_ProviderId_DayOfWeek");

        builder.HasOne(pa => pa.Provider)
            .WithMany()
            .HasForeignKey(pa => pa.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pa => pa.Breaks)
            .WithOne(b => b.ProviderAvailability)
            .HasForeignKey(b => b.ProviderAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureTimeSlot(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<TimeSlot>();

        builder.HasKey(ts => ts.Id);

        builder.Property(ts => ts.Status)
            .HasConversion<int>();

        builder.Property(ts => ts.CreatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(ts => ts.UpdatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(ts => new { ts.ProviderId, ts.StartTime })
            .HasDatabaseName("IX_TimeSlot_ProviderId_StartTime");

        builder.HasIndex(ts => new { ts.ProviderId, ts.Status })
            .HasDatabaseName("IX_TimeSlot_ProviderId_Status");

        builder.HasOne(ts => ts.Provider)
            .WithMany()
            .HasForeignKey(ts => ts.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureBreak(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Break>();

        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAt)
            .HasColumnType("datetimeoffset")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(b => b.ProviderAvailability)
            .WithMany(pa => pa.Breaks)
            .HasForeignKey(b => b.ProviderAvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
