using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Отзывы: менеджер — только просмотр, администратор — модерация.</summary>
public class ReviewsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly SiteCache _cache;
    private readonly NotificationService _notify;

    public ReviewsController(AppDbContext db, SiteCache cache, NotificationService notify)
    {
        _db = db;
        _cache = cache;
        _notify = notify;
    }

    public async Task<IActionResult> Index(ReviewStatus? status)
    {
        var q = _db.Reviews.AsNoTracking().AsQueryable();
        if (status != null) q = q.Where(r => r.Status == status);
        ViewBag.Status = status;
        return View(await q.OrderBy(r => r.Status).ThenByDescending(r => r.CreatedAt).ToListAsync());
    }

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Moderate(int id, ReviewStatus status)
    {
        var r = await _db.Reviews.FindAsync(id);
        if (r == null) return NotFound();
        if (r.Status != status)
        {
            r.Status = status;
            if (status != ReviewStatus.Approved) r.IsFeatured = false;
            _notify.Add(r.UserId, status == ReviewStatus.Approved
                ? "Ваш отзыв опубликован. Спасибо!"
                : "Ваш отзыв не прошёл модерацию.");
            await _db.SaveChangesAsync();
            _cache.ResetReviews();
            Ok($"Отзыв: {Review.StatusTitle(status)}.");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Feature(int id)
    {
        var r = await _db.Reviews.FindAsync(id);
        if (r == null) return NotFound();
        if (r.Status != ReviewStatus.Approved)
        {
            Fail("Выделить можно только опубликованный отзыв.");
        }
        else
        {
            r.IsFeatured = !r.IsFeatured;
            await _db.SaveChangesAsync();
            _cache.ResetReviews();
            Ok(r.IsFeatured ? "Отзыв будет показан на главной." : "Отзыв убран с главной.");
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Reply(int id, string? reply)
    {
        var r = await _db.Reviews.FindAsync(id);
        if (r == null) return NotFound();
        var text = string.IsNullOrWhiteSpace(reply) ? null : reply.Trim();
        if (text?.Length > 1000) text = text[..1000];
        var isNew = text != null && text != r.AdminReply;
        r.AdminReply = text;
        if (isNew) _notify.Add(r.UserId, "Автосервис ответил на ваш отзыв.");
        await _db.SaveChangesAsync();
        _cache.ResetReviews();

        if (text == null) Ok("Ответ удалён.");
        else if (r.Status == ReviewStatus.Approved) Ok("Ответ сохранён и опубликован под отзывом.");
        else Fail("Ответ сохранён, но на сайте его не видно: сам отзыв не опубликован. Нажмите «Опубликовать».");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.Reviews.Where(r => r.Id == id).ExecuteDeleteAsync();
        _cache.ResetReviews();
        Ok("Отзыв удалён.");
        return RedirectToAction(nameof(Index));
    }
}
