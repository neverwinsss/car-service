namespace car_service.Services;

/// <summary>
/// Время автосервиса. Хостинг может работать в любом часовом поясе (часто UTC),
/// поэтому все расчёты расписания ведутся по поясу из настройки "TimeZone" (по умолчанию Москва).
/// </summary>
public static class AppTime
{
    private static TimeZoneInfo _tz = TimeZoneInfo.Local;

    public static void Init(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) return;
        try { _tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId); }
        catch (TimeZoneNotFoundException) { }
        catch (InvalidTimeZoneException) { }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _tz);
    public static DateOnly Today => DateOnly.FromDateTime(Now);

    /// <summary>Перевод UTC-времени из БД в местное для отображения.</summary>
    public static DateTime ToLocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _tz);
}
