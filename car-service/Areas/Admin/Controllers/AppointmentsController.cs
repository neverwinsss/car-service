using System.Globalization;
using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Заявки на обслуживание: доступно администратору и менеджеру.</summary>
public class AppointmentsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly BookingService _booking;
    private readonly NotificationService _notify;

    public AppointmentsController(AppDbContext db, BookingService booking, NotificationService notify)
    {
        _db = db;
        _booking = booking;
        _notify = notify;
    }

    public async Task<IActionResult> Index(AppointmentStatus? status, DateOnly? date, string? q, string period = "upcoming")
    {
        var query = _db.Appointments.AsNoTracking().Include(a => a.Service).AsQueryable();
        var today = AppTime.Today;

        if (date != null)
        {
            query = query.Where(a => a.Date == date);
            period = "";
        }
        else if (period == "upcoming") query = query.Where(a => a.Date >= today);
        else if (period == "past") query = query.Where(a => a.Date < today);

        if (status != null) query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(a => a.ClientName.Contains(term) || a.ClientPhone.Contains(term) || (a.Car != null && a.Car.Contains(term)));
        }

        query = period == "past"
            ? query.OrderByDescending(a => a.Date).ThenByDescending(a => a.Time)
            : query.OrderBy(a => a.Date).ThenBy(a => a.Time);

        ViewBag.Status = status;
        ViewBag.Date = date;
        ViewBag.Q = q;
        ViewBag.Period = period;
        if (date != null) ViewBag.Slots = await _booking.GetSlotsAsync(date.Value);

        return View(await query.Take(300).ToListAsync());
    }

    public async Task<IActionResult> Edit(int id)
    {
        var a = await _db.Appointments.Include(x => x.Service).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();
        ViewBag.Services = await _db.Services.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync();
        return View(a);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, int serviceId, DateOnly date, string time, AppointmentStatus status, string? managerNote)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();

        if (!TimeOnly.TryParseExact(time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
        {
            Fail("Некорректное время.");
            return RedirectToAction(nameof(Edit), new { id });
        }

        // При переносе — проверяем, что новый слот свободен.
        if ((a.Date != date || a.Time != t) && status != AppointmentStatus.Cancelled)
        {
            var err = await _booking.ValidateAsync(date, t, a.Id);
            if (err != null)
            {
                Fail("Перенос невозможен: " + err);
                return RedirectToAction(nameof(Edit), new { id });
            }
            _notify.Add(a.UserId, $"Ваша запись перенесена на {date:dd.MM.yyyy} в {t:HH\\:mm}.");
        }

        var statusChanged = a.Status != status;
        a.ServiceId = serviceId;
        a.Date = date;
        a.Time = t;
        a.Status = status;
        a.ManagerNote = managerNote?.Trim();
        a.UpdatedAt = DateTime.UtcNow;
        if (statusChanged) _notify.AppointmentStatusChanged(a);

        await _db.SaveChangesAsync();
        Ok($"Запись №{a.Id} сохранена.");
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Быстрая смена статуса из списка (подтвердить / выполнено / отменить).</summary>
    [HttpPost]
    public async Task<IActionResult> SetStatus(int id, AppointmentStatus status, string? returnUrl)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return NotFound();
        if (a.Status != status)
        {
            a.Status = status;
            a.UpdatedAt = DateTime.UtcNow;
            _notify.AppointmentStatusChanged(a);
            await _db.SaveChangesAsync();
            Ok($"Запись №{a.Id}: {Appointment.StatusTitle(status)}.");
        }
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.Appointments.Where(a => a.Id == id).ExecuteDeleteAsync();
        Ok("Запись удалена.");
        return RedirectToAction(nameof(Index));
    }
}
