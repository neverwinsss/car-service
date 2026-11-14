using System.ComponentModel.DataAnnotations;

namespace car_service.Models;

/// <summary>
/// Настройки сайта (одна строка в таблице): контакты, кнопка «Позвонить»,
/// тексты «О сервисе», параметры расписания записи.
/// </summary>
public class SiteSettings
{
    public int Id { get; set; }

    // --- Общие ---
    [Required, MaxLength(80)]
    [Display(Name = "Название автосервиса")]
    public string CompanyName { get; set; } = "АвтоПрофи";

    [MaxLength(200)]
    [Display(Name = "Слоган")]
    public string? Slogan { get; set; }

    // --- Контакты ---
    [Required, MaxLength(40)]
    [Display(Name = "Телефон (для отображения)")]
    public string Phone { get; set; } = "";

    [MaxLength(120)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [Required, MaxLength(250)]
    [Display(Name = "Адрес")]
    public string Address { get; set; } = "";

    [MaxLength(200)]
    [Display(Name = "Режим работы (текст)")]
    public string? WorkHoursText { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Ссылка на карту (iframe src, например Яндекс.Карты)")]
    public string? MapEmbedUrl { get; set; }

    [MaxLength(200)]
    [Display(Name = "ВКонтакте")]
    public string? VkUrl { get; set; }

    [MaxLength(200)]
    [Display(Name = "Telegram")]
    public string? TelegramUrl { get; set; }

    [MaxLength(200)]
    [Display(Name = "WhatsApp / MAX")]
    public string? WhatsappUrl { get; set; }

    // --- Кнопка «Позвонить» ---
    [Display(Name = "Показывать кнопку «Позвонить»")]
    public bool CallButtonEnabled { get; set; } = true;

    [Required, MaxLength(40)]
    [Display(Name = "Номер для звонка (tel:)")]
    public string CallPhone { get; set; } = "";

    [MaxLength(40)]
    [Display(Name = "Текст кнопки")]
    public string CallButtonText { get; set; } = "Позвонить";

    [Display(Name = "Плавающая кнопка на мобильных")]
    public bool CallButtonFloating { get; set; } = true;

    // --- Тексты ---
    [MaxLength(800)]
    [Display(Name = "О сервисе — кратко (главная)")]
    public string? AboutShort { get; set; }

    [MaxLength(6000)]
    [Display(Name = "О сервисе — подробно")]
    public string? AboutFull { get; set; }

    [Display(Name = "Лет на рынке")]
    public int YearsOnMarket { get; set; } = 12;

    [Display(Name = "Обслужено автомобилей")]
    public int CarsServiced { get; set; } = 15000;

    [Display(Name = "Гарантия на работы, мес.")]
    public int WarrantyMonths { get; set; } = 12;

    [MaxLength(1000)]
    [Display(Name = "Юридическая информация (подвал)")]
    public string? LegalInfo { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Команда разработчиков (подвал)")]
    public string? DevelopersInfo { get; set; }

    // --- Расписание записи ---
    [Display(Name = "Начало рабочего дня")]
    public TimeOnly WorkStart { get; set; } = new(9, 0);

    [Display(Name = "Конец рабочего дня")]
    public TimeOnly WorkEnd { get; set; } = new(20, 0);

    [Range(15, 240)]
    [Display(Name = "Шаг записи, мин")]
    public int SlotMinutes { get; set; } = 60;

    [Range(1, 50)]
    [Display(Name = "Постов (машин в один слот)")]
    public int SlotCapacity { get; set; } = 3;

    [Range(1, 180)]
    [Display(Name = "Запись вперёд, дней")]
    public int BookingDaysAhead { get; set; } = 30;

    /// <summary>Рабочие дни недели через запятую: 1=Пн … 7=Вс.</summary>
    [MaxLength(20)]
    [Display(Name = "Рабочие дни")]
    public string WorkingDays { get; set; } = "1,2,3,4,5,6";

    public HashSet<DayOfWeek> GetWorkingDays()
    {
        var set = new HashSet<DayOfWeek>();
        foreach (var part in WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (int.TryParse(part, out var n) && n is >= 1 and <= 7)
                set.Add(n == 7 ? DayOfWeek.Sunday : (DayOfWeek)n);
        }
        return set;
    }

    /// <summary>Номер, пригодный для ссылки tel: (только цифры и +).</summary>
    public string CallPhoneHref => "tel:" + new string(CallPhone.Where(c => char.IsDigit(c) || c == '+').ToArray());
}
