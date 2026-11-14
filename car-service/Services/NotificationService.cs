using car_service.Data;
using car_service.Models;

namespace car_service.Services;

/// <summary>Уведомления в личном кабинете (смена статуса записи, модерация отзыва).</summary>
public class NotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public void Add(int? userId, string text)
    {
        if (userId == null) return;
        _db.Notifications.Add(new Notification { UserId = userId.Value, Text = text });
    }

    public void AppointmentStatusChanged(Appointment a)
    {
        var when = $"{a.Date:dd.MM.yyyy} в {a.Time:HH\\:mm}";
        var text = a.Status switch
        {
            AppointmentStatus.Confirmed => $"Ваша запись на {when} подтверждена. Ждём вас!",
            AppointmentStatus.InProgress => $"Автомобиль по записи на {when} принят в работу.",
            AppointmentStatus.Completed => $"Работы по записи на {when} выполнены. Будем рады вашему отзыву!",
            AppointmentStatus.Cancelled => $"Запись на {when} отменена.",
            _ => $"Статус записи на {when}: {Appointment.StatusTitle(a.Status)}."
        };
        Add(a.UserId, text);
    }
}
