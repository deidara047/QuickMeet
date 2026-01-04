using Xunit;
using Moq;
using QuickMeet.Core.Services;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.Entities;

namespace QuickMeet.UnitTests.Services;

public class AvailabilityServiceTests
{
    #region Test Data Constants

    private const int ValidProviderId = 1;
    private const int InvalidProviderId = 999;
    private const int AppointmentDuration = 30;
    private const int BufferMinutes = 10;
    private const int AppointmentDurationSmall = 15;
    private const int BufferSmall = 5;

    private static readonly TimeSpan WorkStartTime = new TimeSpan(9, 0, 0);
    private static readonly TimeSpan WorkEndTime = new TimeSpan(18, 0, 0);
    private static readonly TimeSpan BreakStartTime = new TimeSpan(13, 0, 0);
    private static readonly TimeSpan BreakEndTime = new TimeSpan(14, 0, 0);
    private static readonly TimeSpan InvalidStartTime = new TimeSpan(18, 0, 0);
    private static readonly TimeSpan InvalidEndTime = new TimeSpan(9, 0, 0);

    #endregion

    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<IAvailabilityRepository> _mockAvailabilityRepository;
    private readonly Mock<ITimeSlotRepository> _mockTimeSlotRepository;
    private readonly AvailabilityService _service;

    public AvailabilityServiceTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockAvailabilityRepository = new Mock<IAvailabilityRepository>();
        _mockTimeSlotRepository = new Mock<ITimeSlotRepository>();

        _service = new AvailabilityService(
            _mockProviderRepository.Object,
            _mockAvailabilityRepository.Object,
            _mockTimeSlotRepository.Object
        );
    }

    #region ConfigureAvailabilityAsync - Happy Path

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithValidSingleDayConfig_CreatesAvailabilitySuccessfully()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = WorkStartTime,
                    EndTime = WorkEndTime,
                    Breaks = []
                }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);
        _mockAvailabilityRepository.Setup(r => r.RemoveByProviderIdAsync(ValidProviderId)).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability>());
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        await _service.ConfigureAvailabilityAsync(ValidProviderId, config);

        _mockAvailabilityRepository.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<ProviderAvailability>>(avails =>
                avails.Count() == 1 &&
                avails.First().ProviderId == ValidProviderId &&
                avails.First().DayOfWeek == DayOfWeek.Monday &&
                avails.First().StartTime == WorkStartTime &&
                avails.First().EndTime == WorkEndTime
            )),
            Times.Once,
            "Should add ProviderAvailability with correct values"
        );
    }

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithMultipleDaysConfig_CreatesMultipleAvailabilities()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = true, StartTime = WorkStartTime, EndTime = WorkEndTime, Breaks = [] },
                new() { Day = DayOfWeek.Wednesday, IsWorking = true, StartTime = WorkStartTime, EndTime = WorkEndTime, Breaks = [] },
                new() { Day = DayOfWeek.Friday, IsWorking = true, StartTime = WorkStartTime, EndTime = WorkEndTime, Breaks = [] }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);
        _mockAvailabilityRepository.Setup(r => r.RemoveByProviderIdAsync(ValidProviderId)).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(3);
        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability>());
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        await _service.ConfigureAvailabilityAsync(ValidProviderId, config);

        _mockAvailabilityRepository.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<ProviderAvailability>>(avails =>
                avails.Count() == 3
            )),
            Times.Once,
            "Should add 3 ProviderAvailabilities for 3 working days"
        );
    }

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithBreak_CreatesAvailabilityWithBreak()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = WorkStartTime,
                    EndTime = WorkEndTime,
                    Breaks = new List<BreakConfig>
                    {
                        new() { StartTime = BreakStartTime, EndTime = BreakEndTime }
                    }
                }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);
        _mockAvailabilityRepository.Setup(r => r.RemoveByProviderIdAsync(ValidProviderId)).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability>());
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        await _service.ConfigureAvailabilityAsync(ValidProviderId, config);

        _mockAvailabilityRepository.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<ProviderAvailability>>(avails =>
                avails.Count() == 1 &&
                avails.First().Breaks.Count == 1 &&
                avails.First().Breaks.First().StartTime == BreakStartTime &&
                avails.First().Breaks.First().EndTime == BreakEndTime
            )),
            Times.Once,
            "Should add ProviderAvailability with break information"
        );
    }

    #endregion

    #region ConfigureAvailabilityAsync - Validations

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithNoDaysConfigured_ThrowsInvalidOperationException()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = false, StartTime = null, EndTime = null, Breaks = [] },
                new() { Day = DayOfWeek.Tuesday, IsWorking = false, StartTime = null, EndTime = null, Breaks = [] }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfigureAvailabilityAsync(ValidProviderId, config)
        );

        Assert.Contains("Al menos un día debe estar configurado", exception.Message);
    }

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithNonExistentProvider_ThrowsInvalidOperationException()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = true, StartTime = WorkStartTime, EndTime = WorkEndTime, Breaks = [] }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId)).ReturnsAsync((Provider?)null);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfigureAvailabilityAsync(InvalidProviderId, config)
        );

        Assert.Contains("Profesional no encontrado", exception.Message);
    }

    [Theory]
    [InlineData(null, "09:00")]
    [InlineData("18:00", null)]
    public async Task ConfigureAvailabilityAsync_WithMissingTime_ThrowsInvalidOperationException(string? startStr, string? endStr)
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = startStr == null ? null : TimeSpan.Parse(startStr),
                    EndTime = endStr == null ? null : TimeSpan.Parse(endStr),
                    Breaks = []
                }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfigureAvailabilityAsync(ValidProviderId, config)
        );

        Assert.Contains("Horario inválido", exception.Message);
    }

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithInvalidTimeRange_ThrowsInvalidOperationException()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = InvalidStartTime,
                    EndTime = InvalidEndTime,
                    Breaks = []
                }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfigureAvailabilityAsync(ValidProviderId, config)
        );

        Assert.Contains("hora de inicio debe ser anterior", exception.Message);
    }

    [Fact]
    public async Task ConfigureAvailabilityAsync_WithBreakOutsideWorkingHours_ThrowsInvalidOperationException()
    {
        var config = new AvailabilityConfig
        {
            Days = new List<DayConfig>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = WorkStartTime,
                    EndTime = WorkEndTime,
                    Breaks = new List<BreakConfig>
                    {
                        new() { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0) }
                    }
                }
            },
            AppointmentDurationMinutes = AppointmentDuration,
            BufferMinutes = BufferMinutes
        };

        var provider = new Provider { Id = ValidProviderId, Email = "test@test.com", Username = "test" };
        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId)).ReturnsAsync(provider);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ConfigureAvailabilityAsync(ValidProviderId, config)
        );

        Assert.Contains("break debe estar dentro del horario laboral", exception.Message);
    }

    #endregion

    #region GenerateTimeSlotsAsync - Happy Path

    [Fact]
    public async Task GenerateTimeSlotsAsync_WithSimpleConfiguration_GeneratesSlotsCorrectly()
    {
        var availability = new ProviderAvailability
        {
            ProviderId = ValidProviderId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 30, 0),
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10,
            Breaks = []
        };

        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability> { availability });
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(2);

        var startDate = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddDays(7);

        var slots = await _service.GenerateTimeSlotsAsync(ValidProviderId, startDate, endDate);

        Assert.NotEmpty(slots);
        Assert.All(slots, slot => Assert.Equal(TimeSlotStatus.Available, slot.Status));
    }

    [Fact]
    public async Task GenerateTimeSlotsAsync_WithBreak_SkipsBreakTime()
    {
        var breakItem = new Break
        {
            StartTime = BreakStartTime,
            EndTime = BreakEndTime
        };

        var availability = new ProviderAvailability
        {
            ProviderId = ValidProviderId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = WorkStartTime,
            EndTime = WorkEndTime,
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10,
            Breaks = new List<Break> { breakItem }
        };

        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability> { availability });
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(It.IsAny<int>());

        var startDate = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddDays(7);

        var slots = await _service.GenerateTimeSlotsAsync(ValidProviderId, startDate, endDate);

        var slotsInBreakTime = slots.Where(s =>
            s.StartTime.TimeOfDay >= BreakStartTime &&
            s.StartTime.TimeOfDay < BreakEndTime
        ).ToList();

        Assert.Empty(slotsInBreakTime);
    }

    [Fact]
    public async Task GenerateTimeSlotsAsync_WithMultipleDays_GeneratesSlotsForConfiguredDaysOnly()
    {
        var mondayAvailability = new ProviderAvailability
        {
            ProviderId = ValidProviderId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = WorkStartTime,
            EndTime = new TimeSpan(10, 0, 0),
            AppointmentDurationMinutes = 30,
            BufferMinutes = 0,
            Breaks = []
        };

        var wednesdayAvailability = new ProviderAvailability
        {
            ProviderId = ValidProviderId,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = WorkStartTime,
            EndTime = new TimeSpan(10, 0, 0),
            AppointmentDurationMinutes = 30,
            BufferMinutes = 0,
            Breaks = []
        };

        var availabilities = new List<ProviderAvailability> { mondayAvailability, wednesdayAvailability };

        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(availabilities);
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(It.IsAny<int>());

        var startDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddDays(7);

        var slots = await _service.GenerateTimeSlotsAsync(ValidProviderId, startDate, endDate);

        var slotsByDay = slots.GroupBy(s => s.StartTime.DayOfWeek).ToList();

        Assert.DoesNotContain(DayOfWeek.Tuesday, slotsByDay.Select(g => g.Key));
        Assert.DoesNotContain(DayOfWeek.Thursday, slotsByDay.Select(g => g.Key));
    }

    #endregion

    #region GenerateTimeSlotsAsync - Edge Cases

    [Fact]
    public async Task GenerateTimeSlotsAsync_WithNoAvailabilityConfigured_ReturnsEmptyList()
    {
        _mockTimeSlotRepository.Setup(r => r.RemoveByProviderIdAndDateRangeAsync(ValidProviderId, It.IsAny<DateTimeOffset>())).ReturnsAsync(0);
        _mockAvailabilityRepository.Setup(r => r.GetByProviderIdAsync(ValidProviderId)).ReturnsAsync(new List<ProviderAvailability>());
        _mockTimeSlotRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

        var startDate = new DateTimeOffset(2026, 1, 5, 0, 0, 0, TimeSpan.Zero);
        var endDate = startDate.AddDays(7);

        var slots = await _service.GenerateTimeSlotsAsync(ValidProviderId, startDate, endDate);

        Assert.Empty(slots);
    }

    #endregion

    #region GetAvailableSlotsForDateAsync - Happy Path

    [Fact]
    public async Task GetAvailableSlotsForDateAsync_WithAvailableSlots_ReturnsOnlyAvailableSlots()
    {
        var testDate = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);
        var slots = new List<TimeSlot>
        {
            new()
            {
                ProviderId = ValidProviderId,
                StartTime = testDate.AddHours(9),
                EndTime = testDate.AddHours(9.5),
                Status = TimeSlotStatus.Available
            },
            new()
            {
                ProviderId = ValidProviderId,
                StartTime = testDate.AddHours(10),
                EndTime = testDate.AddHours(10.5),
                Status = TimeSlotStatus.Reserved
            },
            new()
            {
                ProviderId = ValidProviderId,
                StartTime = testDate.AddHours(11),
                EndTime = testDate.AddHours(11.5),
                Status = TimeSlotStatus.Available
            }
        };

        _mockTimeSlotRepository.Setup(r => r.GetAvailableSlotsByProviderAndDateAsync(ValidProviderId, testDate))
            .ReturnsAsync(slots.Where(s => s.Status == TimeSlotStatus.Available).ToList());

        var result = await _service.GetAvailableSlotsForDateAsync(ValidProviderId, testDate);

        Assert.Equal(2, result.Count());
        Assert.All(result, slot => Assert.Equal(TimeSlotStatus.Available, slot.Status));
    }

    [Fact]
    public async Task GetAvailableSlotsForDateAsync_WithNoSlotsForDate_ReturnsEmptyList()
    {
        var testDate = new DateTimeOffset(2026, 1, 6, 0, 0, 0, TimeSpan.Zero);
        _mockTimeSlotRepository.Setup(r => r.GetAvailableSlotsByProviderAndDateAsync(ValidProviderId, testDate))
            .ReturnsAsync(new List<TimeSlot>());

        var result = await _service.GetAvailableSlotsForDateAsync(ValidProviderId, testDate);

        Assert.Empty(result);
    }

    #endregion
}
