using car_service.Data;
using car_service.Models;
using car_service.Models.ViewModels;
using car_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Controllers;

/// <summary>Страница «О сервисе».</summary>
public class AboutController : Controller
{
    private readonly SiteCache _cache;
    public AboutController(SiteCache cache) => _cache = cache;

    public async Task<IActionResult> Index() => View(await _cache.GetTeamAsync());
}

/// <summary>Страница «Услуги».</summary>
public class ServicesController : Controller
{
    private readonly SiteCache _cache;
    public ServicesController(SiteCache cache) => _cache = cache;

    public async Task<IActionResult> Index() => View(await _cache.GetServicesAsync());
}

/// <summary>Страница «Контакты» + форма обратной связи.</summary>
public class ContactsController : Controller
{
    private readonly AppDbContext _db;
    public ContactsController(AppDbContext db) => _db = db;

    public IActionResult Index() => View(new FeedbackViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(FeedbackViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        _db.FeedbackMessages.Add(new FeedbackMessage
        {
            Name = vm.Name.Trim(),
            Contact = vm.Contact.Trim(),
            Message = vm.Message.Trim()
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Сообщение отправлено. Менеджер свяжется с вами в ближайшее время.";
        return RedirectToAction(nameof(Index));
    }
}

/// <summary>Страница «Отзывы»: просмотр опубликованных и добавление нового (с модерацией).</summary>
public class ReviewsController : Controller
{
    private const int PageSize = 10;
    private readonly AppDbContext _db;

    public ReviewsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(int page = 1) => View(await BuildAsync(page, new ReviewFormViewModel()));

    [HttpPost]
    public async Task<IActionResult> Create(ReviewFormViewModel form)
    {
        var userId = User.GetUserId();
        if (userId == null) return Challenge();

        if (!await CanReviewAsync(userId.Value))
            ModelState.AddModelError("", "Оставить отзыв можно после посещения сервиса (хотя бы одна выполненная запись).");

        if (!ModelState.IsValid)
            return View("Index", await BuildAsync(1, form));

        var user = await _db.Users.FindAsync(userId.Value);
        _db.Reviews.Add(new Review
        {
            UserId = userId,
            AuthorName = user!.FullName,
            Rating = form.Rating,
            Text = form.Text.Trim(),
            Status = ReviewStatus.Pending
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Спасибо! Отзыв отправлен на модерацию и появится после проверки.";
        return RedirectToAction(nameof(Index));
    }

    private Task<bool> CanReviewAsync(int userId) =>
        _db.Appointments.AnyAsync(a => a.UserId == userId && a.Status == AppointmentStatus.Completed);

    private async Task<ReviewsPageViewModel> BuildAsync(int page, ReviewFormViewModel form)
    {
        var q = _db.Reviews.AsNoTracking().Where(r => r.Status == ReviewStatus.Approved);
        var total = await q.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, pages);
        var userId = User.GetUserId();

        return new ReviewsPageViewModel
        {
            Reviews = await q.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            AverageRating = total == 0 ? 0 : await q.AverageAsync(r => r.Rating),
            Page = page,
            TotalPages = pages,
            Form = form,
            IsAuthenticated = userId != null,
            CanReview = userId != null && await CanReviewAsync(userId.Value)
        };
    }
}
