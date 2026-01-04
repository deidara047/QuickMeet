using System.Net;
using System.Net.Http.Headers;
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

    private async Task<string> RegisterAndGetToken(string email = "availability@example.com", string username = "availabilityuser", string fullName = "Availability User")
    {
        var registerRequest = new QuickMeet.API.DTOs.Auth.RegisterRequest(
            Email: email,
            Username: username,
            FullName: fullName,
            Password: "ValidPassword123!@",
            PasswordConfirmation: "ValidPassword123!@"
        );

        var response = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        var result = await response.Content.ReadFromJsonAsync<QuickMeet.API.DTOs.Auth.AuthResponse>();
        return result!.AccessToken;
    }

    private void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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
        var token = await RegisterAndGetToken("config1@example.com", "configuser1", "Config User 1");
        SetAuthorizationHeader(token);
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
        var token = await RegisterAndGetToken("config2@example.com", "configuser2", "Config User 2");
        SetAuthorizationHeader(token);
        var config = CreateValidAvailabilityConfig();
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == "config2@example.com"));

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var savedAvailabilities = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(pa => pa.ProviderId == provider!.Id)
                .Include(pa => pa.Breaks)
                .ToListAsync());

        Assert.Equal(3, savedAvailabilities.Count);
    }

    [Fact]
    public async Task ConfigureAvailability_ValidData_GeneratesTimeSlots()
    {
        var token = await RegisterAndGetToken("slots1@example.com", "slotsuser1", "Slots User 1");
        SetAuthorizationHeader(token);
        var config = CreateValidAvailabilityConfig();
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == "slots1@example.com"));

        var response = await Client.PostAsJsonAsync("/api/availability/configure", config);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var timeSlots = await GetFromDatabase(async db =>
            await db.TimeSlots
                .Where(ts => ts.ProviderId == provider!.Id)
                .ToListAsync());

        Assert.NotEmpty(timeSlots);
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
        var token = await RegisterAndGetToken("nowork@example.com", "noworkuser", "No Work User");
        SetAuthorizationHeader(token);

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
        var token = await RegisterAndGetToken("invalidtime@example.com", "invalidtimeuser", "Invalid Time User");
        SetAuthorizationHeader(token);

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
        var token = await RegisterAndGetToken("breakoutside@example.com", "breakoutsideuser", "Break Outside User");
        SetAuthorizationHeader(token);

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
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.token.here");
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
        var token = await RegisterAndGetToken("getconfig@example.com", "getconfiguser", "Get Config User");
        SetAuthorizationHeader(token);
        var config = CreateValidAvailabilityConfig();
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == "getconfig@example.com"));

        await Client.PostAsJsonAsync("/api/availability/configure", config);

        var response = await Client.GetAsync($"/api/availability/{provider!.Id}");

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
        var token = await RegisterAndGetToken("update1@example.com", "updateuser1", "Update User 1");
        SetAuthorizationHeader(token);
        var initialConfig = CreateValidAvailabilityConfig();
        var provider = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Email == "update1@example.com"));

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

        var response = await Client.PutAsJsonAsync($"/api/availability/{provider!.Id}", updatedConfig);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var savedAvailabilities = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(pa => pa.ProviderId == provider.Id)
                .ToListAsync());

        Assert.Equal(2, savedAvailabilities.Count);
    }

    #endregion
}
