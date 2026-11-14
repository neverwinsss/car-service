using System.ComponentModel.DataAnnotations;

namespace car_service.Models;

public enum ReviewStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>Отзыв клиента. Публикуется только после модерации.</summary>
public class Review
{
    public int Id { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(120)]
    public string AuthorName { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [Required, MaxLength(2000)]
    public string Text { get; set; } = "";

    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

    /// <summary>Выделенный отзыв — выводится на главной.</summary>
    public bool IsFeatured { get; set; }

    [MaxLength(1000)]
    public string? AdminReply { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static string StatusTitle(ReviewStatus s) => s switch
    {
        ReviewStatus.Pending => "На модерации",
        ReviewStatus.Approved => "Опубликован",
        ReviewStatus.Rejected => "Отклонён",
        _ => s.ToString()
    };
}

/// <summary>Слайд баннера на главной странице.</summary>
public class Slide
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите заголовок"), MaxLength(150)]
    [Display(Name = "Заголовок")]
    public string Title { get; set; } = "";

    [MaxLength(400)]
    [Display(Name = "Подзаголовок")]
    public string? Subtitle { get; set; }

    [MaxLength(40)]
    [Display(Name = "Метка (например, «Акция»)")]
    public string? Tag { get; set; }

    [MaxLength(60)]
    [Display(Name = "Текст кнопки")]
    public string? ButtonText { get; set; }

    [MaxLength(300)]
    [Display(Name = "Ссылка кнопки")]
    public string? ButtonUrl { get; set; }

    [MaxLength(300)]
    [Display(Name = "Фоновое изображение (URL, необязательно)")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Порядок")]
    public int SortOrder { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}

/// <summary>Сообщение из формы обратной связи.</summary>
public class FeedbackMessage
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [Required, MaxLength(120)]
    public string Contact { get; set; } = "";

    [Required, MaxLength(2000)]
    public string Message { get; set; } = "";

    public bool IsProcessed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Уведомление пользователю в личном кабинете.</summary>
public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    [Required, MaxLength(500)]
    public string Text { get; set; } = "";

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Нерабочий день (праздник, санитарный день) — запись невозможна.</summary>
public class BlockedDate
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    [MaxLength(200)]
    public string? Reason { get; set; }
}

/// <summary>Член команды (сотрудник автосервиса) — выводится на странице «О сервисе».</summary>
public class TeamMember
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите имя"), MaxLength(120)]
    [Display(Name = "Имя")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Укажите должность"), MaxLength(120)]
    [Display(Name = "Должность")]
    public string Position { get; set; } = "";

    [MaxLength(300)]
    [Display(Name = "Описание / квалификация")]
    public string? Description { get; set; }

    [Display(Name = "Стаж, лет")]
    [Range(0, 70)]
    public int Experience { get; set; }

    [Display(Name = "Порядок")]
    public int SortOrder { get; set; }
}
