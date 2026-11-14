using System.Security.Claims;
using car_service.Data;
using car_service.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace car_service.Services;

/// <summary>Создание cookie-аутентификации и её проверка при каждом запросе.</summary>
public static class UserValidator
{
    public const string StampClaim = "stamp";

    public static ClaimsPrincipal CreatePrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new(StampClaim, Stamp(user))
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    /// <summary>
    /// «Отпечаток» пользователя: при смене роли, пароля или блокировке старые cookie становятся недействительными.
    /// </summary>
    private static string Stamp(User u) =>
        $"{u.Role}|{u.IsBlocked}|{u.PasswordHash[^12..]}";

    public static async Task ValidateAsync(CookieValidatePrincipalContext ctx)
    {
        var idStr = ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var id))
        {
            await Reject(ctx);
            return;
        }

        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null || user.IsBlocked || ctx.Principal!.FindFirstValue(StampClaim) != Stamp(user))
        {
            await Reject(ctx);
            return;
        }

        // Имя могло измениться в профиле — обновляем cookie.
        if (ctx.Principal!.FindFirstValue(ClaimTypes.Name) != user.FullName)
        {
            ctx.ReplacePrincipal(CreatePrincipal(user));
            ctx.ShouldRenew = true;
        }
    }

    private static async Task Reject(CookieValidatePrincipalContext ctx)
    {
        ctx.RejectPrincipal();
        await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public static int? GetUserId(this ClaimsPrincipal p) =>
        int.TryParse(p.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
