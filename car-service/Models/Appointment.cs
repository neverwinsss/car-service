using System.ComponentModel.DataAnnotations;

namespace car_service.Models;

public enum AppointmentStatus
{
    New = 0,
    Confirmed = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>Запись клиента на обслуживание.</summary>
public class Appointment
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int ServiceId { get; set; }
    public ServiceItem? Service { get; set; }

    [Required, MaxLength(120)]
    public string ClientName { get; set; } = "";

    [Required, MaxLength(30)]
    public string ClientPhone { get; set; } = "";

    [MaxLength(120)]
    public string? ClientEmail { get; set; }

    [MaxLength(120)]
    public string? Car { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.New;

    /// <summary>Внутренняя заметка менеджера (не видна клиенту).</summary>
    [MaxLength(1000)]
    public string? ManagerNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public static string StatusTitle(AppointmentStatus s) => s switch
    {
        AppointmentStatus.New => "Новая",
        AppointmentStatus.Confirmed => "Подтверждена",
        AppointmentStatus.InProgress => "В работе",
        AppointmentStatus.Completed => "Выполнена",
        AppointmentStatus.Cancelled => "Отменена",
        _ => s.ToString()
    };

    public static string StatusCss(AppointmentStatus s) => s switch
    {
        AppointmentStatus.New => "badge-new",
        AppointmentStatus.Confirmed => "badge-ok",
        AppointmentStatus.InProgress => "badge-info",
        AppointmentStatus.Completed => "badge-done",
        AppointmentStatus.Cancelled => "badge-bad",
        _ => ""
    };
}
