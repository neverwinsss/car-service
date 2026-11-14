using System.ComponentModel.DataAnnotations;

namespace car_service.Models.ViewModels;

public class HomeViewModel
{
    public List<Slide> Slides { get; set; } = new();
    public List<ServiceItem> Services { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public BookingViewModel Booking { get; set; } = new();
}

public class BookingViewModel
{
    [Required(ErrorMessage = "Выберите услугу")]
    [Display(Name = "Услуга")]
    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Выберите дату")]
    [Display(Name = "Дата")]
    public DateOnly? Date { get; set; }

    [Required(ErrorMessage = "Выберите время")]
    [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Выберите время")]
    [Display(Name = "Время")]
    public string? Time { get; set; }

    [Required(ErrorMessage = "Укажите имя"), StringLength(120)]
    [Display(Name = "Ваше имя")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Укажите телефон")]
    [RegularExpression(@"^[\d\s\+\-\(\)]{6,30}$", ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = "";

    [EmailAddress(ErrorMessage = "Некорректный e-mail"), StringLength(120)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }

    [StringLength(120)]
    [Display(Name = "Автомобиль (марка, модель, год)")]
    public string? Car { get; set; }

    [StringLength(1000)]
    [Display(Name = "Комментарий")]
    public string? Comment { get; set; }

    /// <summary>Режим отображения: компактная форма на главной или расширенная.</summary>
    public bool Compact { get; set; }

    public List<ServiceItem> Services { get; set; } = new();
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите e-mail"), EmailAddress(ErrorMessage = "Некорректный e-mail")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Укажите имя"), StringLength(120)]
    [Display(Name = "Имя и фамилия")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Введите e-mail"), EmailAddress(ErrorMessage = "Некорректный e-mail"), StringLength(120)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [RegularExpression(@"^[\d\s\+\-\(\)]{6,30}$", ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль — минимум 6 символов")]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Повторите пароль"), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Повторите пароль")]
    public string ConfirmPassword { get; set; } = "";
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Укажите имя"), StringLength(120)]
    [Display(Name = "Имя и фамилия")]
    public string FullName { get; set; } = "";

    [RegularExpression(@"^[\d\s\+\-\(\)]{6,30}$", ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [StringLength(120)]
    [Display(Name = "Автомобиль")]
    public string? Car { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введите текущий пароль"), DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "Введите новый пароль"), DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль — минимум 6 символов")]
    [Display(Name = "Новый пароль")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Повторите пароль"), DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Повторите новый пароль")]
    public string ConfirmPassword { get; set; } = "";
}

public class CabinetViewModel
{
    public User User { get; set; } = null!;
    public ProfileViewModel Profile { get; set; } = new();
    public ChangePasswordViewModel Password { get; set; } = new();
    public List<Appointment> Appointments { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
    public string Tab { get; set; } = "appointments";
}

public class ReviewFormViewModel
{
    [Range(1, 5, ErrorMessage = "Поставьте оценку от 1 до 5")]
    [Display(Name = "Оценка")]
    public int Rating { get; set; } = 5;

    [Required(ErrorMessage = "Напишите отзыв"), StringLength(2000, MinimumLength = 10, ErrorMessage = "Отзыв — от 10 до 2000 символов")]
    [Display(Name = "Ваш отзыв")]
    public string Text { get; set; } = "";
}

public class ReviewsPageViewModel
{
    public List<Review> Reviews { get; set; } = new();
    public ReviewFormViewModel Form { get; set; } = new();
    public bool IsAuthenticated { get; set; }
    public bool CanReview { get; set; }
    public double AverageRating { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}

public class FeedbackViewModel
{
    [Required(ErrorMessage = "Укажите имя"), StringLength(120)]
    [Display(Name = "Имя")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Укажите телефон или e-mail"), StringLength(120)]
    [Display(Name = "Телефон или e-mail")]
    public string Contact { get; set; } = "";

    [Required(ErrorMessage = "Напишите сообщение"), StringLength(2000, MinimumLength = 5, ErrorMessage = "Сообщение — от 5 до 2000 символов")]
    [Display(Name = "Сообщение")]
    public string Message { get; set; } = "";
}
