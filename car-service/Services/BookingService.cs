using car_service.Data;
using car_service.Models;
using Microsoft.EntityFrameworkCore;

namespace car_service.Services;

public record TimeSlot(TimeOnly Time, int Booked, int Capacity, bool IsPast)
{
    public bool IsAvailable => !IsPast && Booked < Capacity;
    public int Free => Math.Max(0, Capacity - Booked);
}

/// <summary>Расчёт свободных слотов и проверка возможности записи.</summary>
public class BookingService
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public BookingService(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>Возвращает null, если день доступен, иначе — причину.</summary>
    public async Task<string?> CheckDayAsync(DateOnly date)
    {
        var s = await _cache.GetSettingsAsync();
        var today = AppTime.Today;

        if (date < today) return "Нельзя записаться на прошедшую дату.";
        if (date > today.AddDays(s.BookingDaysAhead)) return $"Запись открыта не более чем на {s.BookingDaysAhead} дней вперёд.";
        if (!s.GetWorkingDays().Contains(date.DayOfWeek)) return "В этот день сервис не работает.";

        var blocked = await _db.BlockedDates.AsNoTracking().FirstOrDefaultAsync(b => b.Date == date);
        if (blocked != null) return "Нерабочий день" + (string.IsNullOrWhiteSpace(blocked.Reason) ? "." : $": {blocked.Reason}.");

        return null;
    }

    public async Task<List<TimeSlot>> GetSlotsAsync(DateOnly date, int? excludeAppointmentId = null)
    {
        var s = await _cache.GetSettingsAsync();
        var booked = await _db.Appointments.AsNoTracking()
            .Where(a => a.Date == date && a.Status != AppointmentStatus.Cancelled && a.Id != excludeAppointmentId)
            .GroupBy(a => a.Time)
            .Select(g => new { Time = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Time, x => x.Count);

        var now = AppTime.Now;
        var result = new List<TimeSlot>();
        var step = Math.Max(15, s.SlotMinutes);
        for (var t = s.WorkStart; t < s.WorkEnd; t = t.AddMinutes(step))
        {
            var isPast = date < DateOnly.FromDateTime(now) ||
                         (date == DateOnly.FromDateTime(now) && t <= TimeOnly.FromDateTime(now).AddMinutes(30));
            result.Add(new TimeSlot(t, booked.GetValueOrDefault(t), s.SlotCapacity, isPast));
            if (t.AddMinutes(step) < t) break; // защита от перехода через полночь
        }
        return result;
    }

    /// <summary>Полная проверка даты и времени. null — можно записывать.</summary>
    public async Task<string?> ValidateAsync(DateOnly date, TimeOnly time, int? excludeAppointmentId = null)
    {
        var dayError = await CheckDayAsync(date);
        if (dayError != null) return dayError;

        var slot = (await GetSlotsAsync(date, excludeAppointmentId)).FirstOrDefault(x => x.Time == time);
        if (slot == null) return "Выбранное время вне графика работы.";
        if (slot.IsPast) return "Это время уже прошло.";
        if (!slot.IsAvailable) return "На это время все посты заняты, выберите другое.";
        return null;
    }
}
