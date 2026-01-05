using QuickMeet.Core.DTOs.Auth;

namespace QuickMeet.Core.Interfaces;

/// <summary>
/// Servicio de autenticación responsable de registro, login y verificación de email.
/// Maneja la lógica de negocio de autenticación sin detalles de infraestructura.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registra un nuevo proveedor en el sistema.
    /// </summary>
    Task<(bool Success, string Message, AuthenticationResultDto? Result)> RegisterAsync(
        string email,
        string username,
        string fullName,
        string password);

    /// <summary>
    /// Autentica un proveedor con email y contraseña.
    /// </summary>
    Task<(bool Success, string Message, AuthenticationResultDto? Result)> LoginAsync(
        string email,
        string password);

    /// <summary>
    /// Verifica el email de un proveedor usando un token de verificación.
    /// </summary>
    Task<(bool Success, string Message)> VerifyEmailAsync(string token);

    /// <summary>
    /// Verifica si un email ya existe en el sistema.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);
    
    /// <summary>
    /// Verifica si un username ya existe en el sistema.
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);
}
