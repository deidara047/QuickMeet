using FluentValidation.TestHelper;
using QuickMeet.API.Validators;
using QuickMeet.Core.DTOs.Providers;
using Xunit;

namespace QuickMeet.UnitTests.API.Validators
{
    public class UpdateProviderRequestValidatorTests
    {
        private readonly UpdateProviderRequestValidator _validator = new();

        #region FullName Validation Tests

        [Fact]
        public void Validate_WithNullFullName_IsValid()
        {
            // Arrange - FullName null es válido porque es un optional update
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Validate_WithEmptyFullName_IsInvalid()
        {
            // Arrange - FullName vacío cuando es provisto debe ser válido (solo whitespace)
            var dto = new UpdateProviderDto(
                FullName: "   ",
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Validate_WithTwoCharacterFullName_IsInvalid()
        {
            // Arrange - FullName con 2 caracteres es demasiado corto
            var dto = new UpdateProviderDto(
                FullName: "ab",
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FullName)
                .WithErrorMessage("FullName debe tener entre 3 y 100 caracteres");
        }

        [Fact]
        public void Validate_WithThreeCharacterFullName_IsValid()
        {
            // Arrange - FullName con 3 caracteres es el mínimo válido
            var dto = new UpdateProviderDto(
                FullName: "abc",
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Validate_WithHundredCharacterFullName_IsValid()
        {
            // Arrange - FullName con 100 caracteres es el máximo válido
            var longName = new string('a', 100);
            var dto = new UpdateProviderDto(
                FullName: longName,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void Validate_WithOneHundredOneCharacterFullName_IsInvalid()
        {
            // Arrange - FullName con 101 caracteres es demasiado largo
            var tooLongName = new string('a', 101);
            var dto = new UpdateProviderDto(
                FullName: tooLongName,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FullName)
                .WithErrorMessage("FullName debe tener entre 3 y 100 caracteres");
        }

        [Fact]
        public void Validate_WithValidFullName_IsValid()
        {
            // Arrange - FullName válido
            var dto = new UpdateProviderDto(
                FullName: "Dr. Juan Pérez García",
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        #endregion

        #region Description Validation Tests

        [Fact]
        public void Validate_WithNullDescription_IsValid()
        {
            // Arrange - Description null es válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_WithMaxLengthDescription_IsValid()
        {
            // Arrange - Description con 500 caracteres es válido
            var maxLengthDescription = new string('a', 500);
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: maxLengthDescription,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Validate_WithExceededLengthDescription_IsInvalid()
        {
            // Arrange - Description con 501 caracteres es demasiado largo
            var tooLongDescription = new string('a', 501);
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: tooLongDescription,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage("Description no puede exceder 500 caracteres");
        }

        [Fact]
        public void Validate_WithValidDescription_IsValid()
        {
            // Arrange - Description válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: "Especialista en odontología con 10 años de experiencia",
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        #endregion

        #region AppointmentDurationMinutes Validation Tests

        [Fact]
        public void Validate_WithNullAppointmentDuration_IsValid()
        {
            // Arrange - AppointmentDurationMinutes null es válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.AppointmentDurationMinutes);
        }

        [Fact]
        public void Validate_WithFourteenMinutesDuration_IsInvalid()
        {
            // Arrange - 14 minutos es menor al mínimo (15)
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: 14
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AppointmentDurationMinutes)
                .WithErrorMessage("AppointmentDurationMinutes debe estar entre 15 y 120 minutos");
        }

        [Fact]
        public void Validate_WithFifteenMinutesDuration_IsValid()
        {
            // Arrange - 15 minutos es el mínimo válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: 15
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.AppointmentDurationMinutes);
        }

        [Fact]
        public void Validate_WithSixtyMinutesDuration_IsValid()
        {
            // Arrange - 60 minutos está dentro del rango válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: 60
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.AppointmentDurationMinutes);
        }

        [Fact]
        public void Validate_WithOneHundredTwentyMinutesDuration_IsValid()
        {
            // Arrange - 120 minutos es el máximo válido
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: 120
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveValidationErrorFor(x => x.AppointmentDurationMinutes);
        }

        [Fact]
        public void Validate_WithOneHundredTwentyOneMinutesDuration_IsInvalid()
        {
            // Arrange - 121 minutos es mayor al máximo (120)
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: 121
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AppointmentDurationMinutes)
                .WithErrorMessage("AppointmentDurationMinutes debe estar entre 15 y 120 minutos");
        }

        #endregion

        #region Combined Validation Tests

        [Fact]
        public void Validate_WithAllFieldsNull_IsValid()
        {
            // Arrange - Todos los campos null es válido (no hace cambios)
            var dto = new UpdateProviderDto(
                FullName: null,
                Description: null,
                PhoneNumber: null,
                AppointmentDurationMinutes: null
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithAllValidFields_IsValid()
        {
            // Arrange - Todos los campos con valores válidos
            var dto = new UpdateProviderDto(
                FullName: "Dr. Carlos López",
                Description: "Cardiólogo con experiencia en medicina preventiva",
                PhoneNumber: "+34 91 234 5678",
                AppointmentDurationMinutes: 45
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMultipleValidationErrors_HasAllErrors()
        {
            // Arrange - Múltiples errores: FullName muy corto, Description muy largo, Duration fuera de rango
            var dto = new UpdateProviderDto(
                FullName: "ab",
                Description: new string('a', 501),
                PhoneNumber: null,
                AppointmentDurationMinutes: 10
            );

            // Act & Assert
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
            result.ShouldHaveValidationErrorFor(x => x.Description);
            result.ShouldHaveValidationErrorFor(x => x.AppointmentDurationMinutes);
        }

        #endregion
    }
}
