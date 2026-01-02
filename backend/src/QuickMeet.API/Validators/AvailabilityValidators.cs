using FluentValidation;
using QuickMeet.API.DTOs.Availability;

namespace QuickMeet.API.Validators;

public class AvailabilityConfigValidator : AbstractValidator<AvailabilityConfigDto>
{
    public AvailabilityConfigValidator()
    {
        RuleFor(x => x.Days)
            .NotEmpty()
            .WithMessage("Debe haber al menos un día configurado.");

        RuleFor(x => x.Days)
            .Must(days => days.Any(d => d.IsWorking))
            .WithMessage("Debe haber al menos un día laboral configurado.");

        RuleFor(x => x.AppointmentDurationMinutes)
            .InclusiveBetween(15, 120)
            .WithMessage("La duración de la cita debe estar entre 15 y 120 minutos.");

        RuleFor(x => x.BufferMinutes)
            .InclusiveBetween(0, 60)
            .WithMessage("El buffer debe estar entre 0 y 60 minutos.");

        RuleForEach(x => x.Days)
            .SetValidator(new DayConfigValidator());
    }
}

public class DayConfigValidator : AbstractValidator<DayConfigDto>
{
    public DayConfigValidator()
    {
        When(x => x.IsWorking, () =>
        {
            RuleFor(x => x.StartTime)
                .NotNull()
                .WithMessage("La hora de inicio es requerida para un día laboral.");

            RuleFor(x => x.EndTime)
                .NotNull()
                .WithMessage("La hora de fin es requerida para un día laboral.");

            RuleFor(x => x)
                .Must(x => x.StartTime < x.EndTime)
                .WithMessage("La hora de inicio debe ser anterior a la hora de fin.");
        });

        RuleForEach(x => x.Breaks)
            .SetValidator(new BreakConfigValidator(this));
    }
}

public class BreakConfigValidator : AbstractValidator<BreakDto>
{
    private readonly DayConfigValidator _parent;

    public BreakConfigValidator(DayConfigValidator parent)
    {
        _parent = parent;

        RuleFor(x => x.StartTime)
            .NotEqual(TimeSpan.Zero)
            .WithMessage("La hora de inicio del descanso no puede estar vacía.");

        RuleFor(x => x.EndTime)
            .NotEqual(TimeSpan.Zero)
            .WithMessage("La hora de fin del descanso no puede estar vacía.");

        RuleFor(x => x)
            .Must(x => x.StartTime < x.EndTime)
            .WithMessage("La hora de inicio del descanso debe ser anterior a la hora de fin.");
    }
}
