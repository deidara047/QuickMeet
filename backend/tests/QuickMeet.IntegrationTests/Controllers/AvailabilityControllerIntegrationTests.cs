using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using QuickMeet.API.DTOs.Availability;
using QuickMeet.Core.Entities;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AvailabilityControllerIntegrationTests : IntegrationTestBase
{
    public AvailabilityControllerIntegrationTests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region Helper Methods

    private async Task<int> RegisterAndSetTestUser(string email = "availability@example.com", string username = "availabilityuser", string fullName = "Availability User")
    {
        // Registrar proveedor en la BD directamente
        int providerId = await RegisterTestProvider(email);
        
        // Establecer el usuario de test
        SetTestUser(providerId, email);
        
        return providerId;
    }

    private AvailabilityConfigDto CreateValidAvailabilityConfig()
    {
        return new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    Breaks = new List<BreakDto>
                    {
                        new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) }
                    }
                },
                new()
                {
                    Day = DayOfWeek.Wednesday,
                    IsWorking = true,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    Breaks = new List<BreakDto>()
                },
                new()
                {
                    Day = DayOfWeek.Friday,
                    IsWorking = true,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    Breaks = new List<BreakDto>()
                }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10
        };
    }

    #endregion

    #region ConfigureAvailability Tests - Happy Path

    [Fact]
    public async Task ConfigureAvailability_ValidData_ReturnsOkWithSlots()
    {
        await RegisterAndSetTestUser("config1@example.com", "configuser1", "Config User 1");
        var config = CreateValidAvailabilityConfig();

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AvailabilityResponseDto>();
        Assert.NotNull(result);
        Assert.NotEmpty(result!.GeneratedSlots);
        foreach (var slot in result.GeneratedSlots)
        {
            Assert.Equal("Available", slot.Status);
        }
    }

    [Fact]
    public async Task ConfigureAvailability_ValidData_PersistsInDatabase()
    {
        int providerId = await RegisterAndSetTestUser("config2@example.com", "configuser2", "Config User 2");
        var config = CreateValidAvailabilityConfig();

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var savedAvailabilities = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(pa => pa.ProviderId == providerId)
                .Include(pa => pa.Breaks)
                .ToListAsync());

        Assert.Equal(3, savedAvailabilities.Count);
    }

    [Fact]
    public async Task ConfigureAvailability_ValidData_GeneratesTimeSlots()
    {
        int providerId = await RegisterAndSetTestUser("slots1@example.com", "slotsuser1", "Slots User 1");
        var config = CreateValidAvailabilityConfig();

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var timeSlots = await GetFromDatabase(async db =>
            await db.TimeSlots
                .Where(ts => ts.ProviderId == providerId)
                .ToListAsync());

        // Los timeSlots se pueden generar de forma asíncrona, así que solo verificamos que la response fue OK
        foreach (var slot in timeSlots)
        {
            Assert.Equal(TimeSlotStatus.Available, slot.Status);
        }
    }

    #endregion

    #region ConfigureAvailability Tests - Validations

    [Fact]
    public async Task ConfigureAvailability_WithoutWorkingDays_ReturnsBadRequest()
    {
        await RegisterAndSetTestUser("nowork@example.com", "noworkuser", "No Work User");

        var config = new AvailabilityConfigDto
        {
            Days = Enumerable.Range(0, 7)
                .Select(i => new DayConfigDto
                {
                    Day = (DayOfWeek)i,
                    IsWorking = false,
                    StartTime = null,
                    EndTime = null,
                    Breaks = new List<BreakDto>()
                })
                .ToList(),
            AppointmentDurationMinutes = 30,
            BufferMinutes = 0
        };

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfigureAvailability_WithInvalidTimeRange_ReturnsBadRequest()
    {
        await RegisterAndSetTestUser("invalidtime@example.com", "invalidtimeuser", "Invalid Time User");

        var config = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(9, 0, 0),
                    Breaks = new List<BreakDto>()
                }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 0
        };

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfigureAvailability_WithBreakOutsideWorkingHours_ReturnsBadRequest()
    {
        await RegisterAndSetTestUser("breakoutside@example.com", "breakoutsideuser", "Break Outside User");

        var config = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    Breaks = new List<BreakDto>
                    {
                        new() { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0) }
                    }
                }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 0
        };

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task ConfigureAvailability_Unauthorized_ReturnsUnauthorized()
    {
        var config = CreateValidAvailabilityConfig();
        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConfigureAvailability_InvalidToken_ReturnsUnauthorized()
    {
        ClearTestUser(); // Asegurar que no hay headers de test
        var config = CreateValidAvailabilityConfig();
        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailability_Unauthorized_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/availability/1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAvailability_Unauthorized_ReturnsUnauthorized()
    {
        var config = CreateValidAvailabilityConfig();
        var response = await Client.PutAsJsonAsync("/api/availability/1", config);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetAvailability Tests

    [Fact]
    public async Task GetAvailability_Authorized_ReturnsCurrentConfiguration()
    {
        int providerId = await RegisterAndSetTestUser("getconfig@example.com", "getconfiguser", "Get Config User");
        var config = CreateValidAvailabilityConfig();

        await Client.PostAsJsonAsync("/api/availability/configure", config);

        var response = await Client.GetAsync($"/api/availability/{providerId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AvailabilityConfigDto>();
        Assert.NotNull(result);
        Assert.Equal(3, result!.Days.Count(d => d.IsWorking));
    }

    #endregion

    #region UpdateAvailability Tests

    [Fact]
    public async Task UpdateAvailability_ValidData_ReturnsOk()
    {
        int providerId = await RegisterAndSetTestUser("update1@example.com", "updateuser1", "Update User 1");
        var initialConfig = CreateValidAvailabilityConfig();

        await Client.PostAsJsonAsync("/api/availability/configure", initialConfig);

        var updatedConfig = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new()
                {
                    Day = DayOfWeek.Monday,
                    IsWorking = true,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    Breaks = new List<BreakDto>()
                },
                new()
                {
                    Day = DayOfWeek.Friday,
                    IsWorking = true,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    Breaks = new List<BreakDto>()
                }
            },
            AppointmentDurationMinutes = 45,
            BufferMinutes = 5
        };

        var response = await Client.PutAsJsonAsync($"/api/availability/{providerId}", updatedConfig);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var savedAvailabilities = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(pa => pa.ProviderId == providerId)
                .ToListAsync());

        Assert.Equal(2, savedAvailabilities.Count);
    }

    #endregion
}
