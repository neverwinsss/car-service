using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Контакты, кнопка «Позвонить», тексты сайта (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class SettingsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public SettingsController(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index() => View(await _db.SiteSettings.OrderBy(s => s.Id).FirstAsync());

    [HttpPost]
    public async Task<IActionResult> Index(SiteSettings form)
    {
        var s = await _db.SiteSettings.OrderBy(x => x.Id).FirstAsync();

        foreach (var (name, url) in new[] { (nameof(form.VkUrl), form.VkUrl), (nameof(form.TelegramUrl), form.TelegramUrl), (nameof(form.WhatsappUrl), form.WhatsappUrl), (nameof(form.MapEmbedUrl), form.MapEmbedUrl) })
            if (!string.IsNullOrWhiteSpace(url) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(name, "Ссылка должна начинаться с https://");

        // Поля расписания редактируются на отдельной странице — не валидируем их здесь.
        foreach (var key in ModelState.Keys.Where(k => k is nameof(SiteSettings.WorkStart) or nameof(SiteSettings.WorkEnd)).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid) return View(form);

        s.CompanyName = form.CompanyName.Trim();
        s.Slogan = form.Slogan;
        s.Phone = form.Phone.Trim();
        s.Email = form.Email;
        s.Address = form.Address.Trim();
        s.WorkHoursText = form.WorkHoursText;
        s.MapEmbedUrl = form.MapEmbedUrl;
        s.VkUrl = form.VkUrl;
        s.TelegramUrl = form.TelegramUrl;
        s.WhatsappUrl = form.WhatsappUrl;
        s.CallButtonEnabled = form.CallButtonEnabled;
        s.CallPhone = form.CallPhone.Trim();
        s.CallButtonText = string.IsNullOrWhiteSpace(form.CallButtonText) ? "Позвонить" : form.CallButtonText.Trim();
        s.CallButtonFloating = form.CallButtonFloating;
        s.AboutShort = form.AboutShort;
        s.AboutFull = form.AboutFull;
        s.YearsOnMarket = form.YearsOnMarket;
        s.CarsServiced = form.CarsServiced;
        s.WarrantyMonths = form.WarrantyMonths;
        s.LegalInfo = form.LegalInfo;
        s.DevelopersInfo = form.DevelopersInfo;

        await _db.SaveChangesAsync();
        _cache.ResetSettings();
        Ok("Настройки сохранены.");
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Расписание записи: часы работы, шаг, количество постов, нерабочие дни (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class ScheduleController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public ScheduleController(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Blocked = await _db.BlockedDates.AsNoTracking()
            .Where(b => b.Date >= AppTime.Today.AddDays(-30)).OrderBy(b => b.Date).ToListAsync();
        return View(await _db.SiteSettings.AsNoTracking().OrderBy(s => s.Id).FirstAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Save(TimeOnly workStart, TimeOnly workEnd, int slotMinutes, int slotCapacity, int bookingDaysAhead, int[]? days)
    {
        if (workEnd <= workStart || slotMinutes is < 15 or > 240 || slotCapacity is < 1 or > 50 || bookingDaysAhead is < 1 or > 180)
        {
            Fail("Проверьте значения: конец дня позже начала, шаг 15–240 мин, постов 1–50, дней 1–180.");
            return RedirectToAction(nameof(Index));
        }

        var s = await _db.SiteSettings.OrderBy(x => x.Id).FirstAsync();
        s.WorkStart = workStart;
        s.WorkEnd = workEnd;
        s.SlotMinutes = slotMinutes;
        s.SlotCapacity = slotCapacity;
        s.BookingDaysAhead = bookingDaysAhead;
        s.WorkingDays = string.Join(",", (days ?? Array.Empty<int>()).Where(d => d is >= 1 and <= 7).Distinct().Order());
        await _db.SaveChangesAsync();
        _cache.ResetSettings();
        Ok("Расписание сохранено.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Block(DateOnly date, string? reason)
    {
        if (await _db.BlockedDates.AnyAsync(b => b.Date == date))
        {
            Fail("Эта дата уже отмечена как нерабочая.");
        }
        else
        {
            _db.BlockedDates.Add(new BlockedDate { Date = date, Reason = reason?.Trim() });
            await _db.SaveChangesAsync();
            var count = await _db.Appointments.CountAsync(a => a.Date == date && a.Status != AppointmentStatus.Cancelled);
            Ok(count > 0
                ? $"Дата {date:dd.MM.yyyy} закрыта. Внимание: на неё уже есть записей — {count}, обработайте их вручную."
                : $"Дата {date:dd.MM.yyyy} закрыта для записи.");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(int id)
    {
        await _db.BlockedDates.Where(b => b.Id == id).ExecuteDeleteAsync();
        Ok("Дата снова доступна для записи.");
        return RedirectToAction(nameof(Index));
    }
}
