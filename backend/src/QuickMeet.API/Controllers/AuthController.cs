using Microsoft.AspNetCore.Mvc;
using QuickMeet.Core.DTOs.Auth;
using QuickMeet.Core.Interfaces;

namespace QuickMeet.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IEmailService emailService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Register attempt for {Email}", request.Email);

        var (success, message, result) = await _authenticationService.RegisterAsync(
            request.Email,
            request.Username,
            request.FullName,
            request.Password);

        if (!success)
        {
            _logger.LogWarning("Registration failed: {Message}", message);
            return BadRequest(new { error = message });
        }

        // TODO: Send verification email
        // await _emailService.SendEmailVerificationAsync(request.Email, request.Username, verificationToken);

        var response = new AuthResponse(
            ProviderId: result!.ProviderId,
            Email: result.Email,
            Username: result.Username,
            FullName: result.FullName,
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresAt: result.ExpiresAt);

        _logger.LogInformation("Registration successful for {Email}", request.Email);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for {Email}", request.Email);

        var (success, message, result) = await _authenticationService.LoginAsync(
            request.Email,
            request.Password);

        if (!success)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", request.Email, message);
            return Unauthorized(new { error = message });
        }

        var response = new AuthResponse(
            ProviderId: result!.ProviderId,
            Email: result.Email,
            Username: result.Username,
            FullName: result.FullName,
            AccessToken: result.AccessToken,
            RefreshToken: result.RefreshToken,
            ExpiresAt: result.ExpiresAt);

        _logger.LogInformation("Login successful for {Email}", request.Email);
        return Ok(response);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        _logger.LogInformation("Email verification attempt");

        var (success, message) = await _authenticationService.VerifyEmailAsync(request.Token);

        if (!success)
        {
            _logger.LogWarning("Email verification failed: {Message}", message);
            return BadRequest(new { error = message });
        }

        _logger.LogInformation("Email verified successfully");
        return Ok(new { message = "Email verified successfully" });
    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // TODO: Implement refresh token logic
        return BadRequest(new { error = "Not implemented yet" });
    }
}
