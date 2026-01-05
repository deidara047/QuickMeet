using QuickMeet.Core.Entities;
using QuickMeet.Core.DTOs.Availability;

namespace QuickMeet.Core.Interfaces;

/// <summary>
/// Servicio de disponibilidad responsable de configurar y gestionar slots de tiempo disponibles.
/// Maneja la lógica de negocio de disponibilidad sin detalles de infraestructura.
/// </summary>
public interface IAvailabilityService
{
    /// <summary>
    /// Configura la disponibilidad de un proveedor basada en su horario laboral y descansos.
    /// </summary>
    Task ConfigureAvailabilityAsync(int providerId, AvailabilityConfigDto config);
    
    /// <summary>
    /// Genera slots de tiempo disponibles para un rango de fechas específico.
    /// </summary>
    Task<IEnumerable<TimeSlot>> GenerateTimeSlotsAsync(int providerId, DateTimeOffset startDate, DateTimeOffset endDate);
    
    /// <summary>
    /// Obtiene los slots disponibles para una fecha específica.
    /// </summary>
    Task<IEnumerable<TimeSlot>> GetAvailableSlotsForDateAsync(int providerId, DateTimeOffset date);
    
    /// <summary>
    /// Obtiene la configuración de disponibilidad actual de un proveedor.
    /// </summary>
    Task<AvailabilityConfigDto?> GetProviderAvailabilityAsync(int providerId);
}
