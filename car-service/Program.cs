using System.Globalization;
using car_service.Data;
using car_service.Models;
using car_service.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Культура: русский язык, но десятичный разделитель — точка (для полей type=number) ----------
var culture = (CultureInfo)CultureInfo.GetCultureInfo("ru-RU").Clone();
culture.NumberFormat.NumberDecimalSeparator = ".";
culture.NumberFormat.CurrencyDecimalSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

AppTime.Init(builder.Configuration["TimeZone"]);

// ---------- База данных SQLite ----------
// Файл БД лежит в папке App_Data рядом с приложением (путь можно переопределить в appsettings.json).
var dbPath = builder.Configuration["Database:Path"] ?? "App_Data/carservice.db";
if (!Path.IsPathRooted(dbPath))
    dbPath = Path.Combine(builder.Environment.ContentRootPath, dbPath);
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

// Ключи шифрования cookie храним рядом с БД — иначе после перезапуска на хостинге всех «выкинет» из аккаунтов.
builder.Services.AddDataProtection()
    .SetApplicationName("car-service")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Path.GetDirectoryName(dbPath)!, "keys")));

// ---------- Авторизация: cookie + роли ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.Cookie.Name = "autoprofi.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        o.ExpireTimeSpan = TimeSpan.FromDays(14);
        o.SlidingExpiration = true;
        // Проверяем, что пользователь не заблокирован и его роль не изменилась.
        o.Events.OnValidatePrincipal = UserValidator.ValidateAsync;
    });

builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("Staff", p => p.RequireRole(Roles.Admin, Roles.Manager));
    o.AddPolicy("AdminOnly", p => p.RequireRole(Roles.Admin));
});

// Хеширование паролей: PBKDF2 (HMAC-SHA512, 100 000 итераций) со случайной солью.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ---------- Кеширование и производительность ----------
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});
builder.Services.AddOutputCache(o =>
{
    // Кеш для публичных JSON-эндпоинтов (слоты меняются при записи — короткое время жизни).
    o.AddPolicy("Slots", p => p.Expire(TimeSpan.FromSeconds(10)).SetVaryByQuery("date"));
});

// Не кодировать кириллицу в HTML как &#x...; — страницы меньше и читаемее.
builder.Services.Configure<Microsoft.Extensions.WebEncoders.WebEncoderOptions>(o =>
    o.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All));

builder.Services.AddScoped<SiteCache>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services, app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

// HTTPS-редирект включается только если хостинг даёт сертификат (см. appsettings: UseHttpsRedirection).
if (app.Configuration.GetValue("UseHttpsRedirection", false))
    app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// Статика: ссылки в разметке содержат хеш версии (?v=...), поэтому такие файлы можно кешировать в браузере на год —
// при изменении файла меняется хеш и браузер скачает новую версию.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Query.ContainsKey("v") && Path.HasExtension(ctx.Request.Path.Value))
    {
        ctx.Response.OnStarting(() =>
        {
            if (ctx.Response.StatusCode == StatusCodes.Status200OK)
                ctx.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            return Task.CompletedTask;
        });
    }
    await next();
});
app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
