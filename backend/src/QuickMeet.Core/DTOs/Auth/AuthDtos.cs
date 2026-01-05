namespace QuickMeet.Core.DTOs.Auth;

public record RegisterRequest(
    string Email,
    string Username,
    string FullName,
    string Password,
    string PasswordConfirmation
);

public record LoginRequest(
    string Email,
    string Password
);

public record VerifyEmailRequest(
    string Token
);

public record RefreshTokenRequest(
    string RefreshToken
);

public record AuthResponse(
    int ProviderId,
    string Email,
    string Username,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);

/// <summary>
/// DTO que representa el resultado de una autenticación exitosa.
/// Utilizado internamente por IAuthenticationService para retornar información de autenticación.
/// </summary>
public record AuthenticationResultDto(
    int ProviderId,
    string Email,
    string Username,
    string FullName,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt
);
