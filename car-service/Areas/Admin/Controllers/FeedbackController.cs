using car_service.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Сообщения из формы обратной связи.</summary>
public class FeedbackController : AdminBaseController
{
    private readonly AppDbContext _db;
    public FeedbackController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index() =>
        View(await _db.FeedbackMessages.AsNoTracking()
            .OrderBy(f => f.IsProcessed).ThenByDescending(f => f.CreatedAt).Take(300).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var f = await _db.FeedbackMessages.FindAsync(id);
        if (f == null) return NotFound();
        f.IsProcessed = !f.IsProcessed;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        await _db.FeedbackMessages.Where(f => f.Id == id).ExecuteDeleteAsync();
        Ok("Сообщение удалено.");
        return RedirectToAction(nameof(Index));
    }
}
