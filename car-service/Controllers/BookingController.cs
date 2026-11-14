using System.Globalization;
using System.Security.Claims;
using car_service.Data;
using car_service.Models;
using car_service.Models.ViewModels;
using car_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace car_service.Controllers;

/// <summary>Запись на обслуживание с выбором даты и времени.</summary>
public class BookingController : Controller
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;
    private readonly BookingService _booking;

    public BookingController(AppDbContext db, SiteCache cache, BookingService booking)
    {
        _db = db;
        _cache = cache;
        _booking = booking;
    }

    public async Task<IActionResult> Index(int? serviceId)
    {
        var vm = await PrefillAsync(User, HttpContext, await _cache.GetServicesAsync(), compact: false);
        if (serviceId != null && vm.Services.Any(s => s.Id == serviceId)) vm.ServiceId = serviceId;
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index(BookingViewModel vm)
    {
        var services = await _cache.GetServicesAsync();
        vm.Services = services;
        vm.Compact = false;

        TimeOnly time = default;
        if (vm.Time != null && !TimeOnly.TryParseExact(vm.Time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            ModelState.AddModelError(nameof(vm.Time), "Некорректное время");

        if (vm.ServiceId != null && services.All(s => s.Id != vm.ServiceId))
            ModelState.AddModelError(nameof(vm.ServiceId), "Услуга недоступна");

        if (ModelState.IsValid)
        {
            var error = await _booking.ValidateAsync(vm.Date!.Value, time);
            if (error != null) ModelState.AddModelError(nameof(vm.Time), error);
        }

        if (!ModelState.IsValid) return View(vm);

        var appointment = new Appointment
        {
            UserId = User.GetUserId(),
            ServiceId = vm.ServiceId!.Value,
            Date = vm.Date!.Value,
            Time = time,
            ClientName = vm.Name.Trim(),
            ClientPhone = vm.Phone.Trim(),
            ClientEmail = vm.Email?.Trim(),
            Car = vm.Car?.Trim(),
            Comment = vm.Comment?.Trim()
        };
        _db.Appointments.Add(appointment);
        if (appointment.UserId != null)
            _db.Notifications.Add(new Notification
            {
                UserId = appointment.UserId.Value,
                Text = $"Заявка на {appointment.Date:dd.MM.yyyy} в {appointment.Time:HH\\:mm} принята. Менеджер подтвердит её в ближайшее время."
            });
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Success), new { id = appointment.Id });
    }

    public async Task<IActionResult> Success(int id)
    {
        var a = await _db.Appointments.AsNoTracking().Include(x => x.Service).FirstOrDefaultAsync(x => x.Id == id);
        // Защита от перебора: показываем только недавно созданные заявки.
        if (a == null || a.CreatedAt < DateTime.UtcNow.AddHours(-1)) return RedirectToAction(nameof(Index));
        return View(a);
    }

    /// <summary>JSON со слотами на выбранную дату (используется формой записи).</summary>
    [HttpGet]
    [OutputCache(PolicyName = "Slots")]
    public async Task<IActionResult> Slots(string date)
    {
        if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return BadRequest(new { error = "Некорректная дата" });

        var dayError = await _booking.CheckDayAsync(d);
        if (dayError != null) return Json(new { error = dayError, slots = Array.Empty<object>() });

        var slots = await _booking.GetSlotsAsync(d);
        return Json(new
        {
            error = (string?)null,
            slots = slots.Select(s => new { time = s.Time.ToString("HH:mm"), free = s.Free, available = s.IsAvailable })
        });
    }

    /// <summary>Заготовка формы: для авторизованного пользователя подставляем данные профиля.</summary>
    internal static async Task<BookingViewModel> PrefillAsync(ClaimsPrincipal principal, HttpContext http, List<ServiceItem> services, bool compact)
    {
        var vm = new BookingViewModel { Services = services, Compact = compact };
        var userId = principal.GetUserId();
        if (userId != null)
        {
            var db = http.RequestServices.GetRequiredService<AppDbContext>();
            var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (u != null)
            {
                vm.Name = u.FullName;
                vm.Phone = u.Phone ?? "";
                vm.Email = u.Email;
                vm.Car = u.Car;
            }
        }
        return vm;
    }
}
