using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace car_service.Areas.Admin.Controllers;

/// <summary>Управление пользователями и ролями (только администратор).</summary>
[Authorize(Policy = "AdminOnly")]
public class UsersController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public UsersController(AppDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<IActionResult> Index(string? q, string? role)
    {
        var query = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(u => u.Email.Contains(term) || u.FullName.ToLower().Contains(term) || (u.Phone != null && u.Phone.Contains(term)));
        }
        if (!string.IsNullOrEmpty(role)) query = query.Where(u => u.Role == role);

        ViewBag.Q = q;
        ViewBag.Role = role;
        ViewBag.Counts = await _db.Appointments.Where(a => a.UserId != null)
            .GroupBy(a => a.UserId!.Value).Select(g => new { g.Key, C = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.C);
        return View(await query.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> SetRole(int id, string role)
    {
        if (!Roles.All.Contains(role)) return BadRequest();
        if (id == User.GetUserId())
        {
            Fail("Нельзя изменить собственную роль.");
            return RedirectToAction(nameof(Index));
        }
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        u.Role = role;
        await _db.SaveChangesAsync();
        Ok($"{u.FullName}: роль «{Roles.Title(role)}».");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        if (id == User.GetUserId())
        {
            Fail("Нельзя заблокировать самого себя.");
            return RedirectToAction(nameof(Index));
        }
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        u.IsBlocked = !u.IsBlocked;
        await _db.SaveChangesAsync();
        Ok(u.IsBlocked ? $"{u.FullName} заблокирован(а)." : $"{u.FullName} разблокирован(а).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            Fail("Пароль — минимум 6 символов.");
            return RedirectToAction(nameof(Index));
        }
        var u = await _db.Users.FindAsync(id);
        if (u == null) return NotFound();
        u.PasswordHash = _hasher.HashPassword(u, newPassword);
        await _db.SaveChangesAsync();
        Ok($"Пароль пользователя {u.Email} изменён.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string email, string fullName, string password, string role, string? phone)
    {
        email = (email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(fullName) || !email.Contains('@') || (password ?? "").Length < 6 || !Roles.All.Contains(role))
        {
            Fail("Заполните имя, корректный e-mail, пароль (от 6 символов) и роль.");
            return RedirectToAction(nameof(Index));
        }
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            Fail("Пользователь с таким e-mail уже существует.");
            return RedirectToAction(nameof(Index));
        }
        var user = new User { Email = email, FullName = fullName.Trim(), Phone = phone?.Trim(), Role = role };
        user.PasswordHash = _hasher.HashPassword(user, password!);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        Ok($"Пользователь {email} создан.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == User.GetUserId())
        {
            Fail("Нельзя удалить самого себя.");
            return RedirectToAction(nameof(Index));
        }
        await _db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();
        Ok("Пользователь удалён.");
        return RedirectToAction(nameof(Index));
    }
}
