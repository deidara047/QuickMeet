using QuickMeet.Core.Entities;
using QuickMeet.Core.Interfaces;
using QuickMeet.Core.DTOs.Availability;

namespace QuickMeet.Core.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IProviderRepository _providerRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;

    public AvailabilityService(
        IProviderRepository providerRepository,
        IAvailabilityRepository availabilityRepository,
        ITimeSlotRepository timeSlotRepository)
    {
        _providerRepository = providerRepository;
        _availabilityRepository = availabilityRepository;
        _timeSlotRepository = timeSlotRepository;
    }

    public async Task ConfigureAvailabilityAsync(int providerId, AvailabilityConfigDto config)
    {
        if (config.Days == null || !config.Days.Any(d => d.IsWorking))
        {
            throw new InvalidOperationException("Al menos un día debe estar configurado como laboral.");
        }

        var provider = await _providerRepository.GetByIdAsync(providerId);
        if (provider == null)
        {
            throw new InvalidOperationException("Profesional no encontrado.");
        }

        await _availabilityRepository.RemoveByProviderIdAsync(providerId);

        var newAvailabilities = new List<ProviderAvailability>();

        foreach (var dayConfig in config.Days.Where(d => d.IsWorking))
        {
            if (!dayConfig.StartTime.HasValue || !dayConfig.EndTime.HasValue)
            {
                throw new InvalidOperationException($"Horario inválido para {dayConfig.Day}.");
            }

            if (dayConfig.StartTime >= dayConfig.EndTime)
            {
                throw new InvalidOperationException($"La hora de inicio debe ser anterior a la hora de fin para {dayConfig.Day}.");
            }

            var availability = new ProviderAvailability
            {
                ProviderId = providerId,
                DayOfWeek = dayConfig.Day,
                StartTime = dayConfig.StartTime.Value,
                EndTime = dayConfig.EndTime.Value,
                AppointmentDurationMinutes = config.AppointmentDurationMinutes,
                BufferMinutes = config.BufferMinutes,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (dayConfig.Breaks != null && dayConfig.Breaks.Any())
            {
                foreach (var breakConfig in dayConfig.Breaks)
                {
                    if (breakConfig.StartTime < dayConfig.StartTime || breakConfig.EndTime > dayConfig.EndTime)
                    {
                        throw new InvalidOperationException($"El break debe estar dentro del horario laboral para {dayConfig.Day}.");
                    }

                    availability.Breaks.Add(new Break
                    {
                        StartTime = breakConfig.StartTime,
                        EndTime = breakConfig.EndTime,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }

            newAvailabilities.Add(availability);
        }

        await _availabilityRepository.AddRangeAsync(newAvailabilities);
        await _availabilityRepository.SaveChangesAsync();

        await GenerateTimeSlotsAsync(providerId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(60));
    }

    public async Task<IEnumerable<TimeSlot>> GenerateTimeSlotsAsync(int providerId, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        await _timeSlotRepository.RemoveByProviderIdAndDateRangeAsync(providerId, startDate);

        var availabilities = await _availabilityRepository.GetByProviderIdAsync(providerId);

        if (!availabilities.Any())
        {
            await _timeSlotRepository.SaveChangesAsync();
            return [];
        }

        var generatedSlots = new List<TimeSlot>();
        var currentDate = startDate.Date;
        var endDateOnly = endDate.Date;

        while (currentDate <= endDateOnly)
        {
            var dayOfWeek = currentDate.DayOfWeek;
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

            if (availability != null)
            {
                var breaks = availability.Breaks.OrderBy(b => b.StartTime).ToList();
                var slots = GenerateSlotsForDay(
                    currentDate,
                    availability.StartTime,
                    availability.EndTime,
                    availability.AppointmentDurationMinutes,
                    availability.BufferMinutes,
                    breaks
                );

                generatedSlots.AddRange(slots);
            }

            currentDate = currentDate.AddDays(1);
        }

        await _timeSlotRepository.AddRangeAsync(generatedSlots);
        await _timeSlotRepository.SaveChangesAsync();

        return generatedSlots;
    }

    public async Task<IEnumerable<TimeSlot>> GetAvailableSlotsForDateAsync(int providerId, DateTimeOffset date)
    {
        return await _timeSlotRepository.GetAvailableSlotsByProviderAndDateAsync(providerId, date);
    }

    public async Task<AvailabilityConfigDto?> GetProviderAvailabilityAsync(int providerId)
    {
        var availabilities = await _availabilityRepository.GetByProviderIdAsync(providerId);

        if (!availabilities.Any())
        {
            return null;
        }

        var config = new AvailabilityConfigDto
        {
            AppointmentDurationMinutes = availabilities.First().AppointmentDurationMinutes,
            BufferMinutes = availabilities.First().BufferMinutes,
            Days = []
        };

        for (int i = 0; i < 7; i++)
        {
            var dayOfWeek = (DayOfWeek)i;
            var availability = availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);

            var dayConfig = new DayConfigDto
            {
                Day = dayOfWeek,
                IsWorking = availability != null,
                StartTime = availability?.StartTime,
                EndTime = availability?.EndTime,
                Breaks = availability?.Breaks
                    .Select(b => new BreakDto { StartTime = b.StartTime, EndTime = b.EndTime })
                    .ToList() ?? []
            };

            config.Days.Add(dayConfig);
        }

        return await Task.FromResult(config);
    }

    private List<TimeSlot> GenerateSlotsForDay(
        DateTimeOffset date,
        TimeSpan dayStart,
        TimeSpan dayEnd,
        int appointmentDurationMinutes,
        int bufferMinutes,
        List<Break> breaks)
    {
        var slots = new List<TimeSlot>();
        var currentTime = dayStart;
        var slots_duration = TimeSpan.FromMinutes(appointmentDurationMinutes);
        var slot_buffer = TimeSpan.FromMinutes(bufferMinutes);

        while (currentTime + slots_duration <= dayEnd)
        {
            var slotEnd = currentTime + slots_duration;

            var isInBreak = breaks.Any(b =>
                !(slotEnd <= b.StartTime || currentTime >= b.EndTime));

            if (!isInBreak)
            {
                var slotStartDateTime = new DateTimeOffset(date.Date.Add(currentTime), TimeSpan.Zero);
                var slotEndDateTime = new DateTimeOffset(date.Date.Add(slotEnd), TimeSpan.Zero);

                slots.Add(new TimeSlot
                {
                    ProviderId = 0,
                    StartTime = slotStartDateTime,
                    EndTime = slotEndDateTime,
                    Status = TimeSlotStatus.Available,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            currentTime = slotEnd + slot_buffer;
        }

        return slots;
    }
}
