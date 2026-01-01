using FluentValidation;
using QuickMeet.API.DTOs.Auth;

namespace QuickMeet.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Usuario es requerido")
            .Length(3, 50).WithMessage("Usuario debe tener entre 3 y 50 caracteres")
            .Matches(@"^[a-zA-Z0-9-_]+$").WithMessage("Usuario solo puede contener letras, números, guiones y guiones bajos");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nombre completo es requerido")
            .MaximumLength(256).WithMessage("Nombre completo no puede exceder 256 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Contraseña es requerida")
            .MinimumLength(8).WithMessage("Contraseña debe tener al menos 8 caracteres")
            .Must(p => p.Any(char.IsUpper)).WithMessage("Contraseña debe contener al menos una mayúscula")
            .Must(p => p.Any(char.IsDigit)).WithMessage("Contraseña debe contener al menos un número")
            .Must(p => p.Any(c => !char.IsLetterOrDigit(c))).WithMessage("Contraseña debe contener al menos un carácter especial");

        RuleFor(x => x.PasswordConfirmation)
            .NotEmpty().WithMessage("Confirmación de contraseña es requerida")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email es requerido")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Contraseña es requerida");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token es requerido");
    }
}
