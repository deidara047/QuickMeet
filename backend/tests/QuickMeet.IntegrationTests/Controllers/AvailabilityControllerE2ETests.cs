using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using QuickMeet.Core.DTOs.Availability;
using QuickMeet.IntegrationTests.Common;
using QuickMeet.IntegrationTests.Fixtures;
using Xunit;

namespace QuickMeet.IntegrationTests.Controllers;

public class AvailabilityControllerE2ETests : IntegrationTestBase
{
    public AvailabilityControllerE2ETests(QuickMeetWebApplicationFactory factory) : base(factory) { }

    #region E2E Scenarios

    [Fact]
    public async Task E2E_ConfiguracionCompletaDisponibilidad_Exitosa()
    {
        // SETUP: Registrar proveedor en BD y establecer como usuario de test
        var correo = "disponibilidad@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // PASO 1: Configurar disponibilidad (Lunes-Viernes, 09:00-18:00, break 13:00-14:00)
        var configuracion = CrearConfiguracionValida();
        var respuestaConfiguracion = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);

        Assert.Equal(HttpStatusCode.OK, respuestaConfiguracion.StatusCode);
        var respuestaBody = await respuestaConfiguracion.Content.ReadFromJsonAsync<AvailabilityResponseDto>();
        Assert.NotNull(respuestaBody);

        // PASO 2: Verificar persistencia en BD - que se hayan guardado las disponibilidades
        var disponibilidadesEnBd = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(x => x.ProviderId == proveedorId)
                .ToListAsync()
        );
        Assert.NotEmpty(disponibilidadesEnBd);
        Assert.Equal(5, disponibilidadesEnBd.Count); // 5 días (Lun-Vie)

        // Verificar descansos
        var descansosEnBd = await GetFromDatabase(async db =>
            await db.Breaks
                .Where(x => x.ProviderAvailability!.ProviderId == proveedorId)
                .ToListAsync()
        );
        Assert.NotEmpty(descansosEnBd);
        Assert.Equal(5, descansosEnBd.Count); // 1 descanso por día

        // PASO 3: Obtener configuración existente
        var respuestaObtener = await Client.GetAsync($"/api/availability/{proveedorId}");
        Assert.Equal(HttpStatusCode.OK, respuestaObtener.StatusCode);
        var configObtiene = await respuestaObtener.Content.ReadFromJsonAsync<AvailabilityConfigDto>();
        Assert.NotNull(configObtiene);
        Assert.Equal(30, configObtiene.AppointmentDurationMinutes);
        Assert.Equal(10, configObtiene.BufferMinutes);

        // PASO 4: Actualizar configuración (horario reducido)
        var nuevaConfiguracion = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = true, StartTime = new TimeSpan(10, 0, 0), 
                        EndTime = new TimeSpan(17, 0, 0), Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Tuesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Wednesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Thursday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Friday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Saturday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Sunday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() }
            },
            AppointmentDurationMinutes = 60,
            BufferMinutes = 15
        };

        var respuestaActualizar = await Client.PutAsJsonAsync($"/api/availability/{proveedorId}", nuevaConfiguracion);
        Assert.Equal(HttpStatusCode.OK, respuestaActualizar.StatusCode);

        // PASO 5: Verificar estado final en BD
        var disponibilidadesActualizadas = await GetFromDatabase(async db =>
            await db.ProviderAvailabilities
                .Where(x => x.ProviderId == proveedorId)
                .ToListAsync()
        );
        Assert.Single(disponibilidadesActualizadas); // Solo debe haber 1 día después de actualizar
    }

    [Fact]
    public async Task E2E_SinTokenAutorizacion_DevuelveNoAutorizado()
    {
        // No establecer usuario de test (sin autenticación)
        ClearTestUser();
        
        var configuracion = CrearConfiguracionValida();
        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_ConfiguracionSinDiasLaborales_DevuelveBadRequest()
    {
        // SETUP
        var correo = "nodias@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Todos los días sin trabajar
        var configuracion = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Tuesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Wednesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Thursday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Friday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Saturday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Sunday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10
        };

        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_RangoHorariosInvalido_DevuelveBadRequest()
    {
        // SETUP
        var correo = "horasinvalidas@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Horario invertido (inicio después de fin)
        var configuracion = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new() { Day = DayOfWeek.Monday, IsWorking = true, StartTime = new TimeSpan(18, 0, 0), 
                        EndTime = new TimeSpan(9, 0, 0), Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Tuesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Wednesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Thursday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Friday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Saturday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Sunday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10
        };

        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_DescansoFueraDeHorarioLaboral_DevuelveBadRequest()
    {
        // SETUP
        var correo = "descansomalubicado@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Descanso antes del horario laboral
        var configuracion = new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new() { 
                    Day = DayOfWeek.Monday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(8, 30, 0) } } 
                },
                new() { Day = DayOfWeek.Tuesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Wednesday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Thursday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Friday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Saturday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() },
                new() { Day = DayOfWeek.Sunday, IsWorking = false, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, Breaks = new List<BreakDto>() }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10
        };

        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_TokenProveedorDiferente_DevuelveForbidden()
    {
        // SETUP: Registrar dos proveedores
        var correo1 = "proveedor1@example.com";
        var correo2 = "proveedor2@example.com";
        var proveedorId1 = await RegisterTestProvider(correo1);
        var proveedorId2 = await RegisterTestProvider(correo2);

        // Autenticar como proveedor 1
        SetTestUser(proveedorId1, correo1);

        // Intentar actualizar disponibilidad de proveedor 2 (forbidden)
        var configuracion = CrearConfiguracionValida();
        var respuesta = await Client.PutAsJsonAsync($"/api/availability/{proveedorId2}", configuracion);
        
        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_BufferNegativo_DevuelveBadRequest()
    {
        // SETUP
        var correo = "buffernegativo@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Buffer negativo debe ser rechazado
        var configuracion = CrearConfiguracionValida();
        configuracion.BufferMinutes = -5;
        
        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_DuracionCero_DevuelveBadRequest()
    {
        // SETUP
        var correo = "duracioncero@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Duración cero debe ser rechazada
        var configuracion = CrearConfiguracionValida();
        configuracion.AppointmentDurationMinutes = 0;
        
        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_JSONMalformado_DevuelveBadRequest()
    {
        // SETUP
        var correo = "jsonmalformado@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // JSON inválido
        var jsonInvalido = "{ \"Days\": [invalid json }";
        
        var respuesta = await Client.PostAsync(
            "/api/availability/configure",
            new StringContent(jsonInvalido, System.Text.Encoding.UTF8, "application/json"));
        
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task E2E_TimeSlots_GeneradosEnFormatoUTCISO8601()
    {
        // SETUP
        var correo = "utctest@example.com";
        var proveedorId = await RegisterTestProvider(correo);
        SetTestUser(proveedorId, correo);

        // Configurar disponibilidad
        var configuracion = CrearConfiguracionValida();
        var respuesta = await Client.PostAsJsonAsync("/api/availability/configure", configuracion);
        
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var respuestaBody = await respuesta.Content.ReadFromJsonAsync<AvailabilityResponseDto>();
        Assert.NotNull(respuestaBody);

        // Verificar formato ISO 8601 con offset UTC en respuesta
        if (respuestaBody.GeneratedSlots.Count > 0)
        {
            foreach (var slot in respuestaBody.GeneratedSlots)
            {
                // Formato: "2026-01-15T09:00:00+00:00" o "2026-01-15T09:00:00Z"
                Assert.Matches(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})", slot.StartTime);
                Assert.Matches(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})", slot.EndTime);
                
                // Asegurar que es UTC (termina con Z o +00:00)
                Assert.True(
                    slot.StartTime.EndsWith("Z") || slot.StartTime.EndsWith("+00:00"),
                    $"StartTime debe estar en UTC: {slot.StartTime}");
                Assert.True(
                    slot.EndTime.EndsWith("Z") || slot.EndTime.EndsWith("+00:00"),
                    $"EndTime debe estar en UTC: {slot.EndTime}");
            }
        }

        // Verificar en BD: datetimeoffset con offset UTC
        var slotsEnBd = await GetFromDatabase(async db =>
            await db.TimeSlots
                .Where(x => x.ProviderId == proveedorId)
                .ToListAsync());

        foreach (var slot in slotsEnBd)
        {
            // Offset debe ser 00:00 (UTC)
            Assert.Equal(TimeSpan.Zero, slot.StartTime.Offset);
            Assert.Equal(TimeSpan.Zero, slot.EndTime.Offset);
        }
    }

    #endregion

    #region Private Helpers

    private AvailabilityConfigDto CrearConfiguracionValida()
    {
        return new AvailabilityConfigDto
        {
            Days = new List<DayConfigDto>
            {
                new() 
                { 
                    Day = DayOfWeek.Monday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) } } 
                },
                new() 
                { 
                    Day = DayOfWeek.Tuesday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) } } 
                },
                new() 
                { 
                    Day = DayOfWeek.Wednesday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) } } 
                },
                new() 
                { 
                    Day = DayOfWeek.Thursday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) } } 
                },
                new() 
                { 
                    Day = DayOfWeek.Friday, 
                    IsWorking = true, 
                    StartTime = new TimeSpan(9, 0, 0), 
                    EndTime = new TimeSpan(18, 0, 0), 
                    Breaks = new List<BreakDto> { new() { StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0) } } 
                },
                new() 
                { 
                    Day = DayOfWeek.Saturday, 
                    IsWorking = false, 
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero, 
                    Breaks = new List<BreakDto>() 
                },
                new() 
                { 
                    Day = DayOfWeek.Sunday, 
                    IsWorking = false, 
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero, 
                    Breaks = new List<BreakDto>() 
                }
            },
            AppointmentDurationMinutes = 30,
            BufferMinutes = 10
        };
    }

    #endregion
}
