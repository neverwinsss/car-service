using System.ComponentModel.DataAnnotations;

namespace car_service.Models;

/// <summary>Роли пользователей системы.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";

    public const string Staff = Admin + "," + Manager;

    public static readonly string[] All = { Admin, Manager, User };

    public static string Title(string role) => role switch
    {
        Admin => "Администратор",
        Manager => "Менеджер",
        _ => "Пользователь"
    };
}

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Email { get; set; } = "";

    [Required, MaxLength(120)]
    public string FullName { get; set; } = "";

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(120)]
    public string? Car { get; set; }

    /// <summary>Хеш пароля (PBKDF2, PasswordHasher из ASP.NET Core Identity).</summary>
    [Required]
    public string PasswordHash { get; set; } = "";

    [Required, MaxLength(20)]
    public string Role { get; set; } = Roles.User;

    public bool IsBlocked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Appointment> Appointments { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
}
