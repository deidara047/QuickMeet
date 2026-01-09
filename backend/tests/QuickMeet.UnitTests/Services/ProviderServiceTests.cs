using Xunit;
using Moq;
using QuickMeet.Core.Services;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.Entities;
using QuickMeet.Core.DTOs.Providers;
using Microsoft.Extensions.Logging;

namespace QuickMeet.UnitTests.Services;

/// <summary>
/// Unit Tests para ProviderService
/// Prueba la lógica de negocio sin dependencias externas (BD real, almacenamiento de archivos, etc)
/// Usa mocks puros para aislar el comportamiento del servicio
/// Patrón: Repository Pattern - SOLO se mockea IProviderRepository
/// </summary>
public class ProviderServiceTests
{
    #region Test Data Constants

    private const int ValidProviderId = 1;
    private const int InvalidProviderId = 999;
    private const string ValidEmail = "provider@example.com";
    private const string ValidUsername = "providername";
    private const string ValidFullName = "Provider Test User";
    private const string ValidDescription = "Professional description";
    private const string ValidPhoneNumber = "+34 612 345 678";
    private const int ValidAppointmentDuration = 30;
    private const string ValidPhotoUrl = "https://example.com/photos/provider.jpg";

    #endregion

    private readonly Mock<IProviderRepository> _mockProviderRepository;
    private readonly Mock<ILogger<ProviderService>> _mockLogger;
    private readonly ProviderService _service;

    public ProviderServiceTests()
    {
        _mockProviderRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<ProviderService>>();

        _service = new ProviderService(
            _mockProviderRepository.Object,
            _mockLogger.Object
        );
    }

    #region GetProviderByIdAsync - Happy Path

    [Fact]
    public async Task GetProviderByIdAsync_WithValidProviderId_ReturnsProviderProfile()
    {
        // Arrange
        var provider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            Description = ValidDescription,
            PhoneNumber = ValidPhoneNumber,
            PhotoUrl = ValidPhotoUrl,
            AppointmentDurationMinutes = ValidAppointmentDuration,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(provider);

        // Act
        var result = await _service.GetProviderByIdAsync(ValidProviderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidProviderId, result!.Id);
        Assert.Equal(ValidEmail, result!.Email);
        Assert.Equal(ValidUsername, result!.Username);
        Assert.Equal(ValidFullName, result!.FullName);
        Assert.Equal(ValidDescription, result!.Description);
        Assert.Equal(ValidPhoneNumber, result!.PhoneNumber);
        Assert.Equal(ValidPhotoUrl, result!.PhotoUrl);
        Assert.Equal(ValidAppointmentDuration, result!.AppointmentDurationMinutes);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithValidProviderId_PreservesTimestamps()
    {
        // Arrange
        var createdAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var updatedAt = new DateTimeOffset(2026, 1, 3, 15, 30, 0, TimeSpan.Zero);

        var provider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(provider);

        // Act
        var result = await _service.GetProviderByIdAsync(ValidProviderId);

        // Assert
        Assert.Equal(createdAt, result!.CreatedAt);
        Assert.Equal(updatedAt, result!.UpdatedAt);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithValidProviderId_ReturnsAllProperties()
    {
        // Arrange
        var provider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            Description = ValidDescription,
            PhoneNumber = ValidPhoneNumber,
            PhotoUrl = ValidPhotoUrl,
            AppointmentDurationMinutes = ValidAppointmentDuration,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(provider);

        // Act
        var result = await _service.GetProviderByIdAsync(ValidProviderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidAppointmentDuration, result.AppointmentDurationMinutes);
    }

    #endregion

    #region GetProviderByIdAsync - Not Found

    [Fact]
    public async Task GetProviderByIdAsync_WithNonExistentProviderId_ReturnsNull()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.GetProviderByIdAsync(InvalidProviderId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithNegativeProviderId_ReturnsNull()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.GetByIdAsync(-1))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.GetProviderByIdAsync(-1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithZeroProviderId_ReturnsNull()
    {
        // Arrange
        _mockProviderRepository.Setup(r => r.GetByIdAsync(0))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.GetProviderByIdAsync(0);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region UpdateProviderAsync - Happy Path

    [Fact]
    public async Task UpdateProviderAsync_WithValidData_UpdatesProviderSuccessfully()
    {
        // Arrange
        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = "Old Name",
            Description = "Old Description",
            PhoneNumber = "123456789",
            AppointmentDurationMinutes = 30
        };

        var updateDto = new UpdateProviderDto(
            FullName: ValidFullName,
            Description: ValidDescription,
            PhoneNumber: ValidPhoneNumber,
            AppointmentDurationMinutes: 45
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.UpdateProviderAsync(ValidProviderId, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidProviderId, result.Id);
        Assert.Equal(ValidFullName, result.FullName);
        Assert.Equal(ValidDescription, result.Description);
        Assert.Equal(ValidPhoneNumber, result.PhoneNumber);
        Assert.Equal(45, result.AppointmentDurationMinutes);
    }

    [Fact]
    public async Task UpdateProviderAsync_WithValidData_UpdatesTimestamp()
    {
        // Arrange
        var beforeUpdate = DateTimeOffset.UtcNow;
        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        var updateDto = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.UpdateProviderAsync(ValidProviderId, updateDto);
        var afterUpdate = DateTimeOffset.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True(result.UpdatedAt >= beforeUpdate && result.UpdatedAt <= afterUpdate,
            "UpdatedAt should be updated to current time");
    }

    [Fact]
    public async Task UpdateProviderAsync_WithPartialUpdate_PreservesUnmodifiedFields()
    {
        // Arrange
        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = "Original Name",
            Description = "Original Description",
            PhoneNumber = ValidPhoneNumber,
            AppointmentDurationMinutes = 30
        };

        var updateDto = new UpdateProviderDto(
            FullName: "New Full Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.UpdateProviderAsync(ValidProviderId, updateDto);

        // Assert
        Assert.Equal("New Full Name", result!.FullName);
        Assert.Equal("Original Description", result!.Description);
        Assert.Equal(ValidPhoneNumber, result!.PhoneNumber);
        Assert.Equal(30, result!.AppointmentDurationMinutes);
    }

    [Fact]
    public async Task UpdateProviderAsync_WithValidData_CallsRepositoryUpdate()
    {
        // Arrange
        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            Description = ValidDescription
        };

        var updateDto = new UpdateProviderDto(
            FullName: "Updated Name",
            Description: "Updated Description",
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        await _service.UpdateProviderAsync(ValidProviderId, updateDto);

        // Assert
        _mockProviderRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>()),
            Times.Once,
            "Repository.UpdateAsync should be called once"
        );
    }

    #endregion

    #region UpdateProviderAsync - Not Found

    [Fact]
    public async Task UpdateProviderAsync_WithNonExistentProviderId_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateProviderDto(
            FullName: ValidFullName,
            Description: ValidDescription,
            PhoneNumber: ValidPhoneNumber,
            AppointmentDurationMinutes: 30
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _service.UpdateProviderAsync(InvalidProviderId, updateDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProviderAsync_WithNonExistentProviderId_DoesNotCallRepositoryUpdate()
    {
        // Arrange
        var updateDto = new UpdateProviderDto(
            FullName: ValidFullName,
            Description: ValidDescription,
            PhoneNumber: ValidPhoneNumber,
            AppointmentDurationMinutes: 30
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId))
            .ReturnsAsync((Provider?)null);

        // Act
        await _service.UpdateProviderAsync(InvalidProviderId, updateDto);

        // Assert
        _mockProviderRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>()),
            Times.Never,
            "Repository.UpdateAsync should not be called if provider not found"
        );
    }

    #endregion

    #region UpdateProviderAsync - Business Logic Validations

    [Fact]
    public async Task UpdateProviderAsync_WithSuspendedProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var suspendedProvider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            Status = ProviderStatus.Suspended,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var updateDto = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(suspendedProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateProviderAsync(ValidProviderId, updateDto)
        );
        Assert.Contains("suspendido", exception.Message);
    }

    [Fact]
    public async Task UpdateProviderAsync_WithDeletedProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var deletedProvider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            Username = ValidUsername,
            FullName = ValidFullName,
            Status = ProviderStatus.Deleted,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var updateDto = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(deletedProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateProviderAsync(ValidProviderId, updateDto)
        );
        Assert.Contains("eliminado", exception.Message);
    }

    #endregion

    #region UpdateProviderAsync - Input Validations (Removed)

    // ❌ Los siguientes tests fueron REMOVIDOS porque las validaciones de ENTRADA
    // ahora están en UpdateProviderRequestValidator (API Layer)
    // No es responsabilidad del Service validar formato, longitud, etc.
    
    // UpdateProviderAsync_WithInvalidFullName_ThrowsArgumentException ❌ REMOVIDO
    // UpdateProviderAsync_WithShortFullName_ThrowsArgumentException ❌ REMOVIDO
    // UpdateProviderAsync_WithTooLongFullName_ThrowsArgumentException ❌ REMOVIDO
    // UpdateProviderAsync_WithTooLongDescription_ThrowsArgumentException ❌ REMOVIDO
    // UpdateProviderAsync_WithInvalidAppointmentDuration_ThrowsArgumentException ❌ REMOVIDO

    #endregion

    #region UploadPhotoAsync - Happy Path

    [Fact]
    public async Task UploadPhotoAsync_WithValidPhoto_SavesPhotoUrl()
    {
        // Arrange
        var photoContent = new byte[1024 * 100]; // 100KB
        var photoFileName = "photo.jpg";

        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            Email = ValidEmail
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.UploadPhotoAsync(ValidProviderId, photoContent, photoFileName);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains(ValidProviderId.ToString(), result);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithValidPhoto_UpdatesProviderPhotoUrl()
    {
        // Arrange
        var photoContent = new byte[1024 * 100]; // 100KB
        var photoFileName = "photo.jpg";

        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            Email = ValidEmail
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var photoUrl = await _service.UploadPhotoAsync(ValidProviderId, photoContent, photoFileName);

        // Assert
        _mockProviderRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>()),
            Times.Once,
            "Repository.UpdateAsync should be called to save photo URL"
        );
        Assert.Equal(photoUrl, existingProvider.PhotoUrl);
    }

    [Fact]
    public async Task UploadPhotoAsync_ReplaceExistingPhoto_UpdatesPhotoUrl()
    {
        // Arrange
        var oldPhotoUrl = "https://example.com/old-photo.jpg";
        var newPhotoContent = new byte[1024 * 50]; // 50KB
        var newPhotoFileName = "new-photo.png";

        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            PhotoUrl = oldPhotoUrl
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var newPhotoUrl = await _service.UploadPhotoAsync(ValidProviderId, newPhotoContent, newPhotoFileName);

        // Assert
        Assert.NotEqual(oldPhotoUrl, newPhotoUrl);
        Assert.Equal(newPhotoUrl, existingProvider.PhotoUrl);
    }

    #endregion

    #region UploadPhotoAsync - Business Logic Validations

    [Fact]
    public async Task UploadPhotoAsync_WithSuspendedProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var suspendedProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            Email = ValidEmail,
            Status = ProviderStatus.Suspended,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var photoContent = new byte[1024 * 100];
        var photoFileName = "photo.jpg";

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(suspendedProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UploadPhotoAsync(ValidProviderId, photoContent, photoFileName)
        );
        Assert.Contains("suspendido", exception.Message);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithDeletedProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var deletedProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = ValidFullName,
            Email = ValidEmail,
            Status = ProviderStatus.Deleted,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var photoContent = new byte[1024 * 100];
        var photoFileName = "photo.jpg";

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(deletedProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UploadPhotoAsync(ValidProviderId, photoContent, photoFileName)
        );
        Assert.Contains("eliminado", exception.Message);
    }

    #endregion

    #region UploadPhotoAsync - Input Validations (Removed)

    // ❌ Los siguientes tests fueron REMOVIDOS porque las validaciones de ENTRADA
    // ahora están en UploadPhotoRequestValidator (API Layer)
    // No es responsabilidad del Service validar tamaño, extensión, etc.
    
    // UploadPhotoAsync_WithFileSizeExceedingLimit_ThrowsArgumentException ❌ REMOVIDO
    // UploadPhotoAsync_WithInvalidFileExtension_ThrowsArgumentException ❌ REMOVIDO
    // UploadPhotoAsync_WithValidFileExtensions_Succeeds ❌ REMOVIDO (input validation)
    // UploadPhotoAsync_WithEmptyFileContent_ThrowsArgumentException ❌ REMOVIDO
    // UploadPhotoAsync_WithInvalidFileName_ThrowsArgumentException ❌ REMOVIDO

    #endregion

    #region UploadPhotoAsync - Not Found

    [Fact]
    public async Task UploadPhotoAsync_WithNonExistentProviderId_ThrowsInvalidOperationException()
    {
        // Arrange
        var photoContent = new byte[1024 * 100];
        var photoFileName = "photo.jpg";

        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId))
            .ReturnsAsync((Provider?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UploadPhotoAsync(InvalidProviderId, photoContent, photoFileName)
        );
        Assert.Contains("Provider no encontrado", exception.Message);
    }

    [Fact]
    public async Task UploadPhotoAsync_WithNonExistentProviderId_DoesNotCallRepositoryUpdate()
    {
        // Arrange
        var photoContent = new byte[1024 * 100];
        var photoFileName = "photo.jpg";

        _mockProviderRepository.Setup(r => r.GetByIdAsync(InvalidProviderId))
            .ReturnsAsync((Provider?)null);

        // Act
        try
        {
            await _service.UploadPhotoAsync(InvalidProviderId, photoContent, photoFileName);
        }
        catch { /* Expected exception */ }

        // Assert
        _mockProviderRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>()),
            Times.Never,
            "Repository.UpdateAsync should not be called if provider not found"
        );
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetProviderByIdAsync_WithSuspendedProvider_ReturnsProvider()
    {
        // Arrange - Debería retornar provider incluso si está suspendido
        var suspendedProvider = new Provider
        {
            Id = ValidProviderId,
            Email = ValidEmail,
            FullName = ValidFullName,
            Status = ProviderStatus.Suspended,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(suspendedProvider);

        // Act
        var result = await _service.GetProviderByIdAsync(ValidProviderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ValidFullName, result.FullName);
    }

    [Fact]
    public async Task UpdateProviderAsync_WithAllFieldsNull_DoesNotModifyProvider()
    {
        // Arrange - Actualización con todos los campos null
        var existingProvider = new Provider
        {
            Id = ValidProviderId,
            FullName = "Original Name",
            Description = "Original Description",
            PhoneNumber = "123456789",
            AppointmentDurationMinutes = 30
        };

        var originalFullName = existingProvider.FullName;
        var originalDescription = existingProvider.Description;

        var updateDto = new UpdateProviderDto(
            FullName: null,
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        _mockProviderRepository.Setup(r => r.GetByIdAsync(ValidProviderId))
            .ReturnsAsync(existingProvider);

        // Act
        var result = await _service.UpdateProviderAsync(ValidProviderId, updateDto);

        // Assert
        Assert.Equal(originalFullName, result!.FullName);
        Assert.Equal(originalDescription, result!.Description);
    }

    #endregion
}
