namespace QuickMeet.API.DTOs.Auth;

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
