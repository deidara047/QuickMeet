namespace QuickMeet.Core.DTOs.Availability;

/// <summary>
/// DTO que representa la configuración de disponibilidad completa de un proveedor.
/// Utilizado por IAvailabilityService y Controllers para recibir y retornar configuración de disponibilidad.
/// </summary>
public class AvailabilityConfigDto
{
    public List<DayConfigDto> Days { get; set; } = [];
    
    public int AppointmentDurationMinutes { get; set; } = 30;
    
    public int BufferMinutes { get; set; } = 0;
}
