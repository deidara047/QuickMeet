using QuickMeet.Core.DTOs.Providers;

namespace QuickMeet.Core.Interfaces;

public interface IProviderService
{
    Task<ProviderProfileDto?> GetProviderByIdAsync(int providerId);
    Task<ProviderProfileDto?> UpdateProviderAsync(int providerId, UpdateProviderDto updateDto);
    Task<string> UploadPhotoAsync(int providerId, byte[] fileContent, string fileName);
}
