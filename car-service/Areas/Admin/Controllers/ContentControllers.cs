using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Управление услугами (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class ServicesController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public ServicesController(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index() =>
        View(await _db.Services.AsNoTracking().OrderBy(s => s.SortOrder).ThenBy(s => s.Title).ToListAsync());

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return View(new ServiceItem { SortOrder = await _db.Services.CountAsync() + 1 });
        var s = await _db.Services.FindAsync(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ServiceItem model)
    {
        if (!ServiceItem.Icons.Contains(model.Icon)) model.Icon = "wrench";
        if (!ModelState.IsValid) return View(model);

        if (model.Id == 0) _db.Services.Add(model);
        else _db.Services.Update(model);
        await _db.SaveChangesAsync();
        _cache.ResetServices();
        Ok("Услуга сохранена.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var s = await _db.Services.FindAsync(id);
        if (s == null) return NotFound();

        if (await _db.Appointments.AnyAsync(a => a.ServiceId == id))
        {
            // Нельзя удалить услугу, на которую есть записи — скрываем её.
            s.IsActive = false;
            Ok("На услугу есть записи, поэтому она скрыта с сайта, а не удалена.");
        }
        else
        {
            _db.Services.Remove(s);
            Ok("Услуга удалена.");
        }
        await _db.SaveChangesAsync();
        _cache.ResetServices();
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Слайды баннера на главной (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class SlidesController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public SlidesController(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index() =>
        View(await _db.Slides.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync());

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return View(new Slide { SortOrder = await _db.Slides.CountAsync() + 1 });
        var s = await _db.Slides.FindAsync(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Slide model)
    {
        if (!string.IsNullOrWhiteSpace(model.ButtonUrl) && !IsSafeUrl(model.ButtonUrl))
            ModelState.AddModelError(nameof(model.ButtonUrl), "Ссылка должна начинаться с / или http(s)://");
        if (!string.IsNullOrWhiteSpace(model.ImageUrl) && !IsSafeUrl(model.ImageUrl))
            ModelState.AddModelError(nameof(model.ImageUrl), "Ссылка должна начинаться с / или http(s)://");
        if (!ModelState.IsValid) return View(model);

        if (model.Id == 0) _db.Slides.Add(model);
        else _db.Slides.Update(model);
        await _db.SaveChangesAsync();
        _cache.ResetSlides();
        Ok("Слайд сохранён.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.Slides.Where(s => s.Id == id).ExecuteDeleteAsync();
        _cache.ResetSlides();
        Ok("Слайд удалён.");
        return RedirectToAction(nameof(Index));
    }

    internal static bool IsSafeUrl(string url) =>
        (url.StartsWith('/') && !url.StartsWith("//")) ||
        url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
        url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Сотрудники на странице «О сервисе» (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class TeamController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;

    public TeamController(AppDbContext db, SiteCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index() =>
        View(await _db.TeamMembers.AsNoTracking().OrderBy(s => s.SortOrder).ToListAsync());

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return View(new TeamMember { SortOrder = await _db.TeamMembers.CountAsync() + 1 });
        var s = await _db.TeamMembers.FindAsync(id);
        return s == null ? NotFound() : View(s);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TeamMember model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.Id == 0) _db.TeamMembers.Add(model);
        else _db.TeamMembers.Update(model);
        await _db.SaveChangesAsync();
        _cache.ResetTeam();
        Ok("Сотрудник сохранён.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.TeamMembers.Where(s => s.Id == id).ExecuteDeleteAsync();
        _cache.ResetTeam();
        Ok("Сотрудник удалён.");
        return RedirectToAction(nameof(Index));
    }
}
