using FluentValidation.TestHelper;
using QuickMeet.API.Validators;
using Xunit;

namespace QuickMeet.UnitTests.API.Validators
{
    public class UploadPhotoRequestValidatorTests
    {
        private readonly UploadPhotoRequestValidator _validator = new();

        private const int MaxPhotoSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        #region FileName Validation Tests

        [Fact]
        public void Validate_WithEmptyFileName_IsInvalid()
        {
            // Arrange - FileName vacío no es válido
            var request = new UploadPhotoRequest
            {
                FileName = string.Empty,
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithWhitespaceFileName_IsInvalid()
        {
            // Arrange - FileName con solo espacios no es válido
            var request = new UploadPhotoRequest
            {
                FileName = "   ",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithValidFileName_IsValid()
        {
            // Arrange - FileName válido
            var request = new UploadPhotoRequest
            {
                FileName = "profile-photo.jpg",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        #endregion

        #region FileContent Size Validation Tests

        [Fact]
        public void Validate_WithEmptyFileContent_IsInvalid()
        {
            // Arrange - FileContent vacío no es válido
            var request = new UploadPhotoRequest
            {
                FileName = "photo.jpg",
                FileContent = Array.Empty<byte>()
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileContent);
        }

        [Fact]
        public void Validate_WithSingleByteFileContent_IsValid()
        {
            // Arrange - FileContent con 1 byte es válido (mínimo)
            var request = new UploadPhotoRequest
            {
                FileName = "photo.jpg",
                FileContent = new byte[] { 1 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
        }

        [Fact]
        public void Validate_WithSmallFileContent_IsValid()
        {
            // Arrange - Archivo pequeño (100 KB)
            var smallFile = new byte[100 * 1024];
            var request = new UploadPhotoRequest
            {
                FileName = "photo.jpg",
                FileContent = smallFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
        }

        [Fact]
        public void Validate_WithMaxSizeFileContent_IsValid()
        {
            // Arrange - Archivo exactamente 5 MB es válido
            var maxFile = new byte[MaxPhotoSizeBytes];
            var request = new UploadPhotoRequest
            {
                FileName = "large-photo.jpg",
                FileContent = maxFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
        }

        [Fact]
        public void Validate_WithExceededSizeFileContent_IsInvalid()
        {
            // Arrange - Archivo mayor a 5 MB no es válido
            var oversizeFile = new byte[MaxPhotoSizeBytes + 1];
            var request = new UploadPhotoRequest
            {
                FileName = "oversized-photo.jpg",
                FileContent = oversizeFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileContent)
                .WithErrorMessage("El archivo no puede exceder 5 MB");
        }

        [Fact]
        public void Validate_WithSignificantlyExceededSizeFileContent_IsInvalid()
        {
            // Arrange - Archivo significativamente mayor a 5 MB
            var largeOversizeFile = new byte[10 * 1024 * 1024]; // 10 MB
            var request = new UploadPhotoRequest
            {
                FileName = "very-large-photo.jpg",
                FileContent = largeOversizeFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileContent);
        }

        #endregion

        #region File Extension Validation Tests

        [Fact]
        public void Validate_WithValidJpgExtension_IsValid()
        {
            // Arrange
            var request = new UploadPhotoRequest
            {
                FileName = "photo.jpg",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithValidJpegExtension_IsValid()
        {
            // Arrange
            var request = new UploadPhotoRequest
            {
                FileName = "photo.jpeg",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithValidPngExtension_IsValid()
        {
            // Arrange
            var request = new UploadPhotoRequest
            {
                FileName = "photo.png",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithValidGifExtension_IsValid()
        {
            // Arrange
            var request = new UploadPhotoRequest
            {
                FileName = "photo.gif",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithValidWebpExtension_IsValid()
        {
            // Arrange
            var request = new UploadPhotoRequest
            {
                FileName = "photo.webp",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithInvalidExeExtension_IsInvalid()
        {
            // Arrange - .exe no es una extensión permitida
            var request = new UploadPhotoRequest
            {
                FileName = "malicious.exe",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName)
                .WithErrorMessage("Extensión de archivo no permitida. Solo se aceptan: JPG, PNG, GIF, WebP");
        }

        [Fact]
        public void Validate_WithInvalidPdfExtension_IsInvalid()
        {
            // Arrange - .pdf no es una extensión permitida
            var request = new UploadPhotoRequest
            {
                FileName = "document.pdf",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName)
                .WithErrorMessage("Extensión de archivo no permitida. Solo se aceptan: JPG, PNG, GIF, WebP");
        }

        [Fact]
        public void Validate_WithInvalidTxtExtension_IsInvalid()
        {
            // Arrange - .txt no es una extensión permitida
            var request = new UploadPhotoRequest
            {
                FileName = "notes.txt",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithNoExtension_IsInvalid()
        {
            // Arrange - Archivo sin extensión no es válido
            var request = new UploadPhotoRequest
            {
                FileName = "photo",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
        }

        [Fact]
        public void Validate_WithUppercaseValidExtension_IsValid()
        {
            // Arrange - Extensiones en mayúsculas también deben ser válidas
            var request = new UploadPhotoRequest
            {
                FileName = "photo.JPG",
                FileContent = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.FileName);
        }

        #endregion

        #region Combined Validation Tests

        [Fact]
        public void Validate_WithAllValidFields_IsValid()
        {
            // Arrange - Todos los campos válidos
            var validFile = new byte[500 * 1024]; // 500 KB
            var request = new UploadPhotoRequest
            {
                FileName = "profile-photo.png",
                FileContent = validFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMultipleValidationErrors_HasAllErrors()
        {
            // Arrange - Múltiples errores: FileName vacío y FileContent excedido
            var oversizedFile = new byte[MaxPhotoSizeBytes + 1];
            var request = new UploadPhotoRequest
            {
                FileName = string.Empty,
                FileContent = oversizedFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
            result.ShouldHaveValidationErrorFor(x => x.FileContent);
        }

        [Fact]
        public void Validate_WithInvalidExtensionAndOversizedFile_HasAllErrors()
        {
            // Arrange - Archivo con extensión inválida Y tamaño excedido
            var oversizedFile = new byte[MaxPhotoSizeBytes + 1];
            var request = new UploadPhotoRequest
            {
                FileName = "malicious.exe",
                FileContent = oversizedFile
            };

            // Act & Assert
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.FileName);
            result.ShouldHaveValidationErrorFor(x => x.FileContent);
        }

        #endregion
    }
}
