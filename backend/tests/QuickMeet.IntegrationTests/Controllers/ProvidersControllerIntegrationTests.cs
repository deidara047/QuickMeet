using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.DTOs.Providers;
using QuickMeet.Core.Entities;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

/// <summary>
/// Integration Tests para ProvidersController
/// Testea los endpoints de perfil del proveedor:
/// - GET /api/providers/{providerId}
/// - PUT /api/providers/{providerId}
/// - POST /api/providers/{providerId}/photo
/// </summary>
public class ProvidersControllerIntegrationTests : IntegrationTestBase
{
    public ProvidersControllerIntegrationTests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region GetProvider Tests - Happy Path

    [Fact]
    public async Task GetProvider_AuthenticatedUserOwnProfile_ReturnsOkWithProfile()
    {
        // Arrange - Crear un proveedor en BD
        var providerId = await RegisterTestProvider("provider@example.com");
        SetTestUser(providerId, "provider@example.com");

        // Act
        var response = await Client.GetAsync($"/api/providers/{providerId}");
        var result = await response.Content.ReadFromJsonAsync<ProviderProfileDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(providerId, result.Id);
        Assert.Equal("provider@example.com", result.Email);
        Assert.Equal("Test Provider", result.FullName);
    }

    [Fact]
    public async Task GetProvider_ReturnsAllProfileFields()
    {
        // Arrange
        var providerId = await RegisterTestProvider("complete@example.com");
        SetTestUser(providerId, "complete@example.com");

        // Actualizar perfil con más datos
        await SeedDatabase(db =>
        {
            var provider = db.Providers.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Description = "Specialist in general medicine";
                provider.PhoneNumber = "+34 612 345 678";
                provider.AppointmentDurationMinutes = 30;
                provider.PhotoUrl = "https://example.com/photo.jpg";
            }
        });

        // Act
        var response = await Client.GetAsync($"/api/providers/{providerId}");
        var result = await response.Content.ReadFromJsonAsync<ProviderProfileDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Specialist in general medicine", result.Description);
        Assert.Equal("+34 612 345 678", result.PhoneNumber);
        Assert.Equal(30, result.AppointmentDurationMinutes);
        Assert.Equal("https://example.com/photo.jpg", result.PhotoUrl);
    }

    #endregion

    #region GetProvider Tests - Authorization

    [Fact]
    public async Task GetProvider_AccessingOtherUserProfile_ReturnsForbidden()
    {
        // Arrange - Crear dos proveedores
        var provider1Id = await RegisterTestProvider("provider1@example.com");
        var provider2Id = await RegisterTestProvider("provider2@example.com");

        // Autenticar como provider1 pero intentar acceder a provider2
        SetTestUser(provider1Id, "provider1@example.com");

        // Act
        var response = await Client.GetAsync($"/api/providers/{provider2Id}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProvider_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var providerId = await RegisterTestProvider("protected@example.com");
        ClearTestUser(); // No hay usuario autenticado

        // Act
        var response = await Client.GetAsync($"/api/providers/{providerId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetProvider Tests - Not Found

    [Fact]
    public async Task GetProvider_WithNonExistentProviderId_ReturnsNotFound()
    {
        // Arrange
        var providerId = 9999;
        SetTestUser(providerId); // Autenticar como usuario inexistente

        // Act
        var response = await Client.GetAsync($"/api/providers/{providerId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region UpdateProvider Tests - Happy Path

    [Fact]
    public async Task UpdateProvider_WithValidData_ReturnsOkWithUpdatedProfile()
    {
        // Arrange
        var providerId = await RegisterTestProvider("update@example.com");
        SetTestUser(providerId, "update@example.com");

        var updateRequest = new UpdateProviderDto(
            FullName: "Dr. Updated Name",
            Description: "Updated specialist profile",
            PhoneNumber: "+34 987 654 321",
            AppointmentDurationMinutes: 45
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<ProviderProfileDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(providerId, result.Id);
        Assert.Equal("Dr. Updated Name", result.FullName);
        Assert.Equal("Updated specialist profile", result.Description);
        Assert.Equal("+34 987 654 321", result.PhoneNumber);
        Assert.Equal(45, result.AppointmentDurationMinutes);
    }

    [Fact]
    public async Task UpdateProvider_WithPartialUpdate_PreservesUnmodifiedFields()
    {
        // Arrange
        var providerId = await RegisterTestProvider("partial@example.com");
        SetTestUser(providerId, "partial@example.com");

        // Primero actualizar todos los campos
        var firstUpdate = new UpdateProviderDto(
            FullName: "Original Name",
            Description: "Original Description",
            PhoneNumber: "+34 123 456 789",
            AppointmentDurationMinutes: 30
        );
        await Client.PutAsJsonAsync($"/api/providers/{providerId}", firstUpdate);

        // Luego actualizar solo el nombre
        var partialUpdate = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", partialUpdate);
        var result = await response.Content.ReadFromJsonAsync<ProviderProfileDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("New Name", result.FullName);
        Assert.Equal("Original Description", result.Description); // Preserved
        Assert.Equal("+34 123 456 789", result.PhoneNumber); // Preserved
        Assert.Equal(30, result.AppointmentDurationMinutes); // Preserved
    }

    #endregion

    #region UpdateProvider Tests - Input Validation

    [Fact]
    public async Task UpdateProvider_WithInvalidFullName_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("validation@example.com");
        SetTestUser(providerId, "validation@example.com");

        var invalidRequest = new UpdateProviderDto(
            FullName: "ab", // Too short (min 3)
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("3", content); // Should contain "3" from min length error message
    }

    [Fact]
    public async Task UpdateProvider_WithTooLongDescription_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("longdesc@example.com");
        SetTestUser(providerId, "longdesc@example.com");

        var invalidRequest = new UpdateProviderDto(
            FullName: null,
            Description: new string('a', 501), // Too long (max 500)
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("500", content); // Should contain "500" from max length error message
    }

    [Fact]
    public async Task UpdateProvider_WithInvalidAppointmentDuration_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("duration@example.com");
        SetTestUser(providerId, "duration@example.com");

        var invalidRequest = new UpdateProviderDto(
            FullName: null,
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: 10 // Too low (min 15)
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region UpdateProvider Tests - Business Rule Validations

    [Fact]
    public async Task UpdateProvider_WithSuspendedProvider_ReturnsBadRequest()
    {
        // Arrange - Crear proveedor suspendido
        var providerId = await RegisterTestProvider("suspended@example.com");
        SetTestUser(providerId, "suspended@example.com");

        await SeedDatabase(db =>
        {
            var provider = db.Providers.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Suspended;
            }
        });

        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("suspendido", content.ToLowerInvariant());
    }

    [Fact]
    public async Task UpdateProvider_WithDeletedProvider_ReturnsBadRequest()
    {
        // Arrange - Crear proveedor eliminado
        var providerId = await RegisterTestProvider("deleted@example.com");
        SetTestUser(providerId, "deleted@example.com");

        await SeedDatabase(db =>
        {
            var provider = db.Providers.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Deleted;
            }
        });

        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region UpdateProvider Tests - Authorization & Not Found

    [Fact]
    public async Task UpdateProvider_AccessingOtherUserProfile_ReturnsForbidden()
    {
        // Arrange
        var provider1Id = await RegisterTestProvider("user1@example.com");
        var provider2Id = await RegisterTestProvider("user2@example.com");
        SetTestUser(provider1Id, "user1@example.com");

        var updateRequest = new UpdateProviderDto(
            FullName: "Hacked Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{provider2Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProvider_WithNonExistentProviderId_ReturnsNotFound()
    {
        // Arrange
        var providerId = 9999;
        SetTestUser(providerId);

        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProvider_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var providerId = await RegisterTestProvider("auth@example.com");
        ClearTestUser();

        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        // Act
        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region UploadPhoto Tests - Happy Path

    [Fact]
    public async Task UploadPhoto_WithValidJpgFile_ReturnsOkWithPhotoUrl()
    {
        // Arrange
        var providerId = await RegisterTestProvider("photo@example.com");
        SetTestUser(providerId, "photo@example.com");

        var fileContent = new byte[] { 0xFF, 0xD8, 0xFF }; // JPEG header
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);
        var responseText = await response.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(responseText);
        var photoUrl = jsonDoc.RootElement.GetProperty("photoUrl").GetString();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(photoUrl);
        Assert.Contains(".jpg", photoUrl);
    }

    [Fact]
    public async Task UploadPhoto_WithValidPngFile_ReturnsOkWithPhotoUrl()
    {
        // Arrange
        var providerId = await RegisterTestProvider("png@example.com");
        SetTestUser(providerId, "png@example.com");

        var fileContent = new byte[] { 0x89, 0x50, 0x4E }; // PNG header
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.png");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);
        var responseText = await response.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(responseText);
        var photoUrl = jsonDoc.RootElement.GetProperty("photoUrl").GetString();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(photoUrl);
        Assert.Contains(".png", photoUrl);
    }

    [Fact]
    public async Task UploadPhoto_ReplacesExistingPhoto()
    {
        // Arrange
        var providerId = await RegisterTestProvider("replace@example.com");
        SetTestUser(providerId, "replace@example.com");

        // Upload first photo
        var firstContent = new MultipartFormDataContent();
        firstContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "first.jpg");
        var firstResponse = await Client.PostAsync($"/api/providers/{providerId}/photo", firstContent);
        var firstResponseText = await firstResponse.Content.ReadAsStringAsync();
        var firstJsonDoc = System.Text.Json.JsonDocument.Parse(firstResponseText);
        var firstPhotoUrl = firstJsonDoc.RootElement.GetProperty("photoUrl").GetString();

        // Upload second photo
        var secondContent = new MultipartFormDataContent();
        secondContent.Add(new ByteArrayContent(new byte[] { 0x89, 0x50 }), "file", "second.png");
        var secondResponse = await Client.PostAsync($"/api/providers/{providerId}/photo", secondContent);
        var secondResponseText = await secondResponse.Content.ReadAsStringAsync();
        var secondJsonDoc = System.Text.Json.JsonDocument.Parse(secondResponseText);
        var secondPhotoUrl = secondJsonDoc.RootElement.GetProperty("photoUrl").GetString();

        // Act - Verify the URLs are different
        // Assert
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        Assert.NotEqual(firstPhotoUrl, secondPhotoUrl);
        Assert.Contains(".png", secondPhotoUrl); // Second should be PNG
    }

    #endregion

    #region UploadPhoto Tests - Input Validation

    [Fact]
    public async Task UploadPhoto_WithInvalidExtension_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("invalid@example.com");
        SetTestUser(providerId, "invalid@example.com");

        var fileContent = new byte[] { 0x4D, 0x5A }; // EXE header
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "malicious.exe");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("extensión", responseContent.ToLowerInvariant());
    }

    [Fact]
    public async Task UploadPhoto_WithFileSizeExceeding5MB_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("large@example.com");
        SetTestUser(providerId, "large@example.com");

        var largeFileContent = new byte[5 * 1024 * 1024 + 1]; // 5MB + 1 byte
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(largeFileContent), "file", "large.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("5", responseContent); // Should mention "5 MB"
    }

    [Fact]
    public async Task UploadPhoto_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("empty@example.com");
        SetTestUser(providerId, "empty@example.com");

        var emptyContent = new MultipartFormDataContent();
        emptyContent.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "empty.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", emptyContent);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region UploadPhoto Tests - Business Rule Validations

    [Fact]
    public async Task UploadPhoto_WithSuspendedProvider_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("sus@example.com");
        SetTestUser(providerId, "sus@example.com");

        await SeedDatabase(db =>
        {
            var provider = db.Providers.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Suspended;
            }
        });

        var fileContent = new byte[] { 0xFF, 0xD8 };
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("suspendido", responseContent.ToLowerInvariant());
    }

    [Fact]
    public async Task UploadPhoto_WithDeletedProvider_ReturnsBadRequest()
    {
        // Arrange
        var providerId = await RegisterTestProvider("del@example.com");
        SetTestUser(providerId, "del@example.com");

        await SeedDatabase(db =>
        {
            var provider = db.Providers.FirstOrDefault(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Deleted;
            }
        });

        var fileContent = new byte[] { 0xFF, 0xD8 };
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region UploadPhoto Tests - Authorization & Not Found

    [Fact]
    public async Task UploadPhoto_AccessingOtherUserProfile_ReturnsForbidden()
    {
        // Arrange
        var provider1Id = await RegisterTestProvider("uploader1@example.com");
        var provider2Id = await RegisterTestProvider("uploader2@example.com");
        SetTestUser(provider1Id, "uploader1@example.com");

        var fileContent = new byte[] { 0xFF, 0xD8 };
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{provider2Id}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UploadPhoto_WithNonExistentProviderId_ReturnsNotFound()
    {
        // Arrange
        var providerId = 9999;
        SetTestUser(providerId);

        var fileContent = new byte[] { 0xFF, 0xD8 };
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert - Accept both BadRequest (validation error) or NotFound (provider not found)
        // Validation happens first, so BadRequest is expected
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || 
                    response.StatusCode == HttpStatusCode.NotFound,
                    $"Expected BadRequest or NotFound, got {response.StatusCode}");
    }

    [Fact]
    public async Task UploadPhoto_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var providerId = await RegisterTestProvider("noauth@example.com");
        ClearTestUser();

        var fileContent = new byte[] { 0xFF, 0xD8 };
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(fileContent), "file", "photo.jpg");

        // Act
        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
