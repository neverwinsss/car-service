using car_service.Data;
using car_service.Models;
using car_service.Models.ViewModels;
using car_service.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Controllers;

/// <summary>Регистрация, вход и личный кабинет.</summary>
public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public AccountController(AppDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    // ---------------- Вход ----------------

    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        var result = user == null
            ? PasswordVerificationResult.Failed
            : _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            // Одинаковое сообщение — чтобы не раскрывать, существует ли e-mail.
            ModelState.AddModelError("", "Неверный e-mail или пароль.");
            return View(vm);
        }
        if (user!.IsBlocked)
        {
            ModelState.AddModelError("", "Учётная запись заблокирована. Обратитесь к администратору.");
            return View(vm);
        }
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, vm.Password);
            await _db.SaveChangesAsync();
        }

        await SignInAsync(user, vm.RememberMe);

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return LocalRedirect(vm.ReturnUrl);
        return user.Role is Roles.Admin or Roles.Manager
            ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
            : RedirectToAction(nameof(Index));
    }

    // ---------------- Регистрация ----------------

    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        var email = vm.Email.Trim().ToLowerInvariant();
        if (ModelState.IsValid && await _db.Users.AnyAsync(u => u.Email == email))
            ModelState.AddModelError(nameof(vm.Email), "Пользователь с таким e-mail уже зарегистрирован.");

        if (!ModelState.IsValid) return View(vm);

        var user = new User
        {
            Email = email,
            FullName = vm.FullName.Trim(),
            Phone = vm.Phone?.Trim(),
            Role = Roles.User
        };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.Notifications.Add(new Notification { UserId = user.Id, Text = "Добро пожаловать! Здесь будут появляться уведомления о ваших записях." });
        // Привязываем ранее оформленные без регистрации записи с тем же e-mail.
        var guestAppointments = await _db.Appointments.Where(a => a.UserId == null && a.ClientEmail == email).ToListAsync();
        guestAppointments.ForEach(a => a.UserId = user.Id);
        await _db.SaveChangesAsync();

        await SignInAsync(user, true);
        TempData["Success"] = "Регистрация прошла успешно.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();

    // ---------------- Личный кабинет ----------------

    [Authorize]
    public async Task<IActionResult> Index(string tab = "appointments") => View(await BuildCabinetAsync(tab));

    [Authorize, HttpPost]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] ProfileViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var cab = await BuildCabinetAsync("profile");
            cab.Profile = vm;
            return View("Index", cab);
        }

        var user = await CurrentUserAsync();
        user.FullName = vm.FullName.Trim();
        user.Phone = vm.Phone?.Trim();
        user.Car = vm.Car?.Trim();
        await _db.SaveChangesAsync();
        await SignInAsync(user, true);

        TempData["Success"] = "Профиль сохранён.";
        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel vm)
    {
        var user = await CurrentUserAsync();
        if (ModelState.IsValid &&
            _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.CurrentPassword) == PasswordVerificationResult.Failed)
            ModelState.AddModelError("Password.CurrentPassword", "Текущий пароль указан неверно.");

        if (!ModelState.IsValid)
            return View("Index", await BuildCabinetAsync("profile"));

        user.PasswordHash = _hasher.HashPassword(user, vm.NewPassword);
        await _db.SaveChangesAsync();
        await SignInAsync(user, true);

        TempData["Success"] = "Пароль изменён.";
        return RedirectToAction(nameof(Index), new { tab = "profile" });
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userId = User.GetUserId();
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (a == null) return NotFound();

        if (a.Status is AppointmentStatus.New or AppointmentStatus.Confirmed && a.Date >= AppTime.Today)
        {
            a.Status = AppointmentStatus.Cancelled;
            a.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Запись отменена.";
        }
        else
        {
            TempData["Error"] = "Эту запись уже нельзя отменить.";
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> ReadNotifications()
    {
        var userId = User.GetUserId();
        await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        return RedirectToAction(nameof(Index), new { tab = "notifications" });
    }

    // ---------------- Вспомогательное ----------------

    private async Task<User> CurrentUserAsync()
    {
        var id = User.GetUserId() ?? throw new InvalidOperationException();
        return await _db.Users.FirstAsync(u => u.Id == id);
    }

    private async Task<CabinetViewModel> BuildCabinetAsync(string tab)
    {
        var user = await CurrentUserAsync();
        return new CabinetViewModel
        {
            User = user,
            Tab = tab,
            Profile = new ProfileViewModel { FullName = user.FullName, Phone = user.Phone, Car = user.Car },
            Appointments = await _db.Appointments.AsNoTracking().Include(a => a.Service)
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.Date).ThenByDescending(a => a.Time).ToListAsync(),
            Notifications = await _db.Notifications.AsNoTracking()
                .Where(n => n.UserId == user.Id).OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(),
            Reviews = await _db.Reviews.AsNoTracking()
                .Where(r => r.UserId == user.Id).OrderByDescending(r => r.CreatedAt).ToListAsync()
        };
    }

    private Task SignInAsync(User user, bool persistent) =>
        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            UserValidator.CreatePrincipal(user),
            new AuthenticationProperties { IsPersistent = persistent });
}
