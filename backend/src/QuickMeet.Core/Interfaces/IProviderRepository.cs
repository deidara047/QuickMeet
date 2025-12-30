using QuickMeet.Core.Entities;

namespace QuickMeet.Core.Interfaces;

/// <summary>
/// Repository para operaciones de datos con Providers
/// Abstrae el acceso directo a DbContext
/// </summary>
public interface IProviderRepository
{
    /// <summary>
    /// Obtiene un provider por su email
    /// </summary>
    Task<Provider?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene un provider por su ID
    /// </summary>
    Task<Provider?> GetByIdAsync(int id);

    /// <summary>
    /// Verifica si existe un provider con ese email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Verifica si existe un provider con ese username
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    /// Agrega un nuevo provider
    /// </summary>
    Task AddAsync(Provider provider);

    /// <summary>
    /// Actualiza un provider existente
    /// </summary>
    Task UpdateAsync(Provider provider);

    /// <summary>
    /// Obtiene un provider por username
    /// </summary>
    Task<Provider?> GetByUsernameAsync(string username);
}
