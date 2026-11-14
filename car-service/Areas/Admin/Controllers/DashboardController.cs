using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Staff")]
public abstract class AdminBaseController : Controller
{
    protected void Ok(string message) => TempData["Success"] = message;
    protected void Fail(string message) => TempData["Error"] = message;
}

public class DashboardController : AdminBaseController
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var today = AppTime.Today;
        ViewBag.NewAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.New);
        ViewBag.TodayAppointments = await _db.Appointments.CountAsync(a => a.Date == today && a.Status != AppointmentStatus.Cancelled);
        ViewBag.PendingReviews = await _db.Reviews.CountAsync(r => r.Status == ReviewStatus.Pending);
        ViewBag.NewFeedback = await _db.FeedbackMessages.CountAsync(f => !f.IsProcessed);
        ViewBag.UsersCount = await _db.Users.CountAsync();

        var upcoming = await _db.Appointments.AsNoTracking().Include(a => a.Service)
            .Where(a => a.Date >= today && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.Date).ThenBy(a => a.Time).Take(10).ToListAsync();
        return View(upcoming);
    }
}
