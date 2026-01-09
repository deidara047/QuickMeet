using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.DTOs.Providers;
using QuickMeet.Core.Entities;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

/// <summary>
/// E2E Tests para ProvidersController
/// Testea flujos de negocio completos:
/// - Crear perfil → Actualizar → Subir foto
/// - Verificar persistencia en BD
/// - Validaciones de negocio (provider suspendido, eliminado, etc.)
/// </summary>
public class ProvidersControllerE2ETests : IntegrationTestBase
{
    public ProvidersControllerE2ETests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region E2E Scenarios

    /// <summary>
    /// Flujo completo: Registrar → Obtener Perfil → Actualizar → Subir Foto
    /// Verifica persistencia en BD después de cada operación
    /// </summary>
    [Fact]
    public async Task E2E_FlujoCompletoPerfilYFoto_Exitoso()
    {
        // ==================== SETUP ====================
        var email = "completo@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // ==================== PASO 1: Obtener perfil inicial ====================
        var respuestaObtener1 = await Client.GetAsync($"/api/providers/{providerId}");
        Assert.Equal(HttpStatusCode.OK, respuestaObtener1.StatusCode);
        var perfilInicial = await respuestaObtener1.Content.ReadFromJsonAsync<ProviderProfileDto>();
        Assert.NotNull(perfilInicial);
        Assert.Equal(providerId, perfilInicial.Id);
        Assert.Equal(email, perfilInicial.Email);

        // Verificar en BD que provider existe
        var providerEnBd1 = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId)
        );
        Assert.NotNull(providerEnBd1);
        Assert.Null(providerEnBd1.PhotoUrl);

        // ==================== PASO 2: Actualizar perfil ====================
        var updateRequest = new UpdateProviderDto(
            FullName: "Dr. Juan García Médico",
            Description: "Especialista en medicina general con 15 años de experiencia",
            PhoneNumber: "+34 612 345 678",
            AppointmentDurationMinutes: 45
        );

        var respuestaUpdate = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, respuestaUpdate.StatusCode);
        var perfilActualizado = await respuestaUpdate.Content.ReadFromJsonAsync<ProviderProfileDto>();
        Assert.NotNull(perfilActualizado);
        Assert.Equal("Dr. Juan García Médico", perfilActualizado.FullName);
        Assert.Equal("Especialista en medicina general con 15 años de experiencia", perfilActualizado.Description);
        Assert.Equal("+34 612 345 678", perfilActualizado.PhoneNumber);
        Assert.Equal(45, perfilActualizado.AppointmentDurationMinutes);

        // Verificar en BD que cambios persisten
        var providerEnBd2 = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId)
        );
        Assert.NotNull(providerEnBd2);
        Assert.Equal("Dr. Juan García Médico", providerEnBd2.FullName);
        Assert.Equal(45, providerEnBd2.AppointmentDurationMinutes);

        // ==================== PASO 3: Obtener perfil actualizado ====================
        var respuestaObtener2 = await Client.GetAsync($"/api/providers/{providerId}");
        Assert.Equal(HttpStatusCode.OK, respuestaObtener2.StatusCode);
        var perfilObtenido = await respuestaObtener2.Content.ReadFromJsonAsync<ProviderProfileDto>();
        Assert.NotNull(perfilObtenido);
        Assert.Equal("Dr. Juan García Médico", perfilObtenido.FullName);

        // ==================== PASO 4: Subir foto ====================
        var fotoContent = new MultipartFormDataContent();
        fotoContent.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF }), "file", "profile.jpg");

        var respuestaFoto = await Client.PostAsync($"/api/providers/{providerId}/photo", fotoContent);
        Assert.Equal(HttpStatusCode.OK, respuestaFoto.StatusCode);
        var fotoResponse = await respuestaFoto.Content.ReadAsStringAsync();
        var jsonDoc = System.Text.Json.JsonDocument.Parse(fotoResponse);
        var photoUrl = jsonDoc.RootElement.GetProperty("photoUrl").GetString();
        Assert.NotNull(photoUrl);
        Assert.Contains("photo", photoUrl.ToLower());

        // Verificar en BD que foto se guardó
        var providerEnBd3 = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId)
        );
        Assert.NotNull(providerEnBd3);
        Assert.Equal(photoUrl, providerEnBd3.PhotoUrl);

        // ==================== PASO 5: Obtener perfil con foto ====================
        var respuestaObtener3 = await Client.GetAsync($"/api/providers/{providerId}");
        Assert.Equal(HttpStatusCode.OK, respuestaObtener3.StatusCode);
        var perfilFinal = await respuestaObtener3.Content.ReadFromJsonAsync<ProviderProfileDto>();
        Assert.NotNull(perfilFinal);
        Assert.Equal(photoUrl, perfilFinal.PhotoUrl);
    }

    /// <summary>
    /// No se puede actualizar un perfil de provider suspendido
    /// Verificar validación de NEGOCIO
    /// </summary>
    [Fact]
    public async Task E2E_ActualizarProviderSuspendido_DevuelveBadRequest()
    {
        // SETUP
        var email = "suspended@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Suspender el provider directamente en BD
        await SeedDatabase(async db =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Suspended;
                db.Providers.Update(provider);
                await db.SaveChangesAsync();
            }
        });

        // Intentar actualizar
        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert - Debe rechazar porque está suspendido
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// No se puede subir foto a un provider suspendido
    /// Verificar validación de NEGOCIO
    /// </summary>
    [Fact]
    public async Task E2E_SubirFotoProviderSuspendido_DevuelveBadRequest()
    {
        // SETUP
        var email = "suspended-photo@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Suspender provider
        await SeedDatabase(async db =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Suspended;
                db.Providers.Update(provider);
                await db.SaveChangesAsync();
            }
        });

        // Intentar subir foto
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "photo.jpg");

        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// No se puede actualizar un perfil de provider eliminado
    /// Verificar validación de NEGOCIO
    /// </summary>
    [Fact]
    public async Task E2E_ActualizarProviderEliminado_DevuelveBadRequest()
    {
        // SETUP
        var email = "deleted@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Eliminar el provider directamente en BD
        await SeedDatabase(async db =>
        {
            var provider = await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
            if (provider != null)
            {
                provider.Status = ProviderStatus.Deleted;
                db.Providers.Update(provider);
                await db.SaveChangesAsync();
            }
        });

        // Intentar actualizar
        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Reemplazar foto existente con una nueva
    /// Verificar que la foto anterior se sobrescribe
    /// </summary>
    [Fact]
    public async Task E2E_ReemplazarFoto_Exitoso()
    {
        // SETUP
        var email = "replace-photo@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // PASO 1: Subir primera foto
        var content1 = new MultipartFormDataContent();
        content1.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "photo1.jpg");
        var response1 = await Client.PostAsync($"/api/providers/{providerId}/photo", content1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var responseText1 = await response1.Content.ReadAsStringAsync();
        var jsonDoc1 = System.Text.Json.JsonDocument.Parse(responseText1);
        var photoUrl1 = jsonDoc1.RootElement.GetProperty("photoUrl").GetString();

        // Verificar en BD
        var providerEnBd1 = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId)
        );
        Assert.Equal(photoUrl1, providerEnBd1!.PhotoUrl);

        // PASO 2: Subir segunda foto
        var content2 = new MultipartFormDataContent();
        content2.Add(new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E }), "file", "photo2.png");
        var response2 = await Client.PostAsync($"/api/providers/{providerId}/photo", content2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var responseText2 = await response2.Content.ReadAsStringAsync();
        var jsonDoc2 = System.Text.Json.JsonDocument.Parse(responseText2);
        var photoUrl2 = jsonDoc2.RootElement.GetProperty("photoUrl").GetString();

        // PASO 3: Verificar que la foto fue reemplazada
        Assert.NotEqual(photoUrl1, photoUrl2);
        var providerEnBd2 = await GetFromDatabase(async db =>
            await db.Providers.FirstOrDefaultAsync(p => p.Id == providerId)
        );
        Assert.Equal(photoUrl2, providerEnBd2!.PhotoUrl);
    }

    /// <summary>
    /// Validaciones de entrada: nombres muy cortos o muy largos
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesDeEntrada_NombreInvalido()
    {
        // SETUP
        var email = "validation@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Intentar con nombre muy corto
        var updateRequest = new UpdateProviderDto(
            FullName: "AB", // Muy corto
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert - Debe rechazar por validación de entrada
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Validaciones de entrada: descripción muy larga
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesDeEntrada_DescripcionTooLong()
    {
        // SETUP
        var email = "long-desc@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Descripción de más de 500 caracteres
        var longDescription = new string('A', 501);
        var updateRequest = new UpdateProviderDto(
            FullName: "Valid Name",
            Description: longDescription,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );

        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Validaciones de entrada: duración de cita inválida
    /// Solo se aceptan entre 15 y 120 minutos
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesDeEntrada_DuracionInvalida()
    {
        // SETUP
        var email = "invalid-duration@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Intento con duración menor al mínimo
        var updateRequest = new UpdateProviderDto(
            FullName: "Valid Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: 10 // Menor a 15 minutos (inválido)
        );

        var response = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Validaciones de archivo: extensión no permitida
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesFoto_ExtensionNoPermitida()
    {
        // SETUP
        var email = "invalid-ext@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Intentar con archivo .txt
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }), "file", "photo.txt");

        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Validaciones de archivo: archivo vacío
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesFoto_ArchivoVacio()
    {
        // SETUP
        var email = "empty-file@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Intentar con archivo vacío
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "photo.jpg");

        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Validaciones de archivo: archivo muy grande (>5MB)
    /// </summary>
    [Fact]
    public async Task E2E_ValidacionesFoto_ArchivoMuyGrande()
    {
        // SETUP
        var email = "large-file@example.com";
        var providerId = await RegisterTestProvider(email);
        SetTestUser(providerId, email);

        // Crear un archivo de 6MB
        var largeFile = new byte[6 * 1024 * 1024];
        for (int i = 0; i < largeFile.Length; i++)
        {
            largeFile[i] = 0xFF; // Simular contenido JPEG
        }

        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(largeFile), "file", "large.jpg");

        var response = await Client.PostAsync($"/api/providers/{providerId}/photo", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Sin autenticación: todos los endpoints requieren token
    /// </summary>
    [Fact]
    public async Task E2E_SinAutenticacion_DevuelveUnauthorized()
    {
        // SETUP - No establecer usuario de test
        ClearTestUser();
        var providerId = 1;

        // PASO 1: Obtener sin auth
        var responseGet = await Client.GetAsync($"/api/providers/{providerId}");
        Assert.Equal(HttpStatusCode.Unauthorized, responseGet.StatusCode);

        // PASO 2: Actualizar sin auth
        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );
        var responsePut = await Client.PutAsJsonAsync($"/api/providers/{providerId}", updateRequest);
        Assert.Equal(HttpStatusCode.Unauthorized, responsePut.StatusCode);

        // PASO 3: Subir foto sin auth
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "photo.jpg");
        var responsePost = await Client.PostAsync($"/api/providers/{providerId}/photo", content);
        Assert.Equal(HttpStatusCode.Unauthorized, responsePost.StatusCode);
    }

    /// <summary>
    /// Intentar acceder al perfil de otro provider
    /// Verificar check de autorización
    /// </summary>
    [Fact]
    public async Task E2E_AccesoDePerfil_OtroProvider_ReturnsForbidden()
    {
        // SETUP
        var email1 = "provider1@example.com";
        var email2 = "provider2@example.com";
        var providerId1 = await RegisterTestProvider(email1);
        var providerId2 = await RegisterTestProvider(email2);

        // Autenticar como provider1
        SetTestUser(providerId1, email1);

        // PASO 1: Intentar obtener perfil de provider2
        var responseGet = await Client.GetAsync($"/api/providers/{providerId2}");
        Assert.Equal(HttpStatusCode.Forbidden, responseGet.StatusCode);

        // PASO 2: Intentar actualizar perfil de provider2
        var updateRequest = new UpdateProviderDto(
            FullName: "Hacked Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );
        var responsePut = await Client.PutAsJsonAsync($"/api/providers/{providerId2}", updateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, responsePut.StatusCode);

        // PASO 3: Intentar subir foto al perfil de provider2
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 0xFF, 0xD8 }), "file", "photo.jpg");
        var responsePost = await Client.PostAsync($"/api/providers/{providerId2}/photo", content);
        Assert.Equal(HttpStatusCode.Forbidden, responsePost.StatusCode);
    }

    /// <summary>
    /// Provider no existente
    /// Todos los endpoints deben devolver NotFound para providers inexistentes
    /// </summary>
    [Fact]
    public async Task E2E_ProviderNoExistente_ReturnsNotFound()
    {
        // SETUP
        var nonExistentId = 99999;
        SetTestUser(nonExistentId); // Autenticar como usuario inexistente

        // PASO 1: Obtener perfil de provider no existente
        var responseGet = await Client.GetAsync($"/api/providers/{nonExistentId}");
        Assert.Equal(HttpStatusCode.NotFound, responseGet.StatusCode);

        // PASO 2: Actualizar provider no existente
        var updateRequest = new UpdateProviderDto(
            FullName: "New Name",
            Description: null,
            PhoneNumber: null,
            AppointmentDurationMinutes: null
        );
        var responsePut = await Client.PutAsJsonAsync($"/api/providers/{nonExistentId}", updateRequest);
        Assert.Equal(HttpStatusCode.NotFound, responsePut.StatusCode);
    }

    #endregion
}
