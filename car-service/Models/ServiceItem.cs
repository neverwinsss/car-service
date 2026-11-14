using System.ComponentModel.DataAnnotations;

namespace car_service.Models;

/// <summary>Услуга автосервиса.</summary>
public class ServiceItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Укажите название"), MaxLength(120)]
    [Display(Name = "Название")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Укажите краткое описание"), MaxLength(300)]
    [Display(Name = "Краткое описание")]
    public string ShortDescription { get; set; } = "";

    [MaxLength(4000)]
    [Display(Name = "Подробное описание")]
    public string? Description { get; set; }

    [Range(0, 10_000_000, ErrorMessage = "Некорректная цена")]
    [Display(Name = "Цена от, ₽")]
    public decimal PriceFrom { get; set; }

    [Range(10, 1440, ErrorMessage = "От 10 до 1440 минут")]
    [Display(Name = "Длительность, мин")]
    public int DurationMinutes { get; set; } = 60;

    [Display(Name = "Иконка")]
    [MaxLength(30)]
    public string Icon { get; set; } = "wrench";

    [Display(Name = "Порядок сортировки")]
    public int SortOrder { get; set; }

    [Display(Name = "Показывать на сайте")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Показывать на главной")]
    public bool IsFeatured { get; set; }

    public static readonly string[] Icons = { "wrench", "engine", "oil", "tire", "brake", "battery", "diag", "ac", "body", "suspension" };
}
