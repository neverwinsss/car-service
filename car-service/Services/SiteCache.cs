using car_service.Data;
using car_service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace car_service.Services;

/// <summary>
/// Кеширование редко меняющихся данных (настройки, услуги, слайды, отзывы на главной) в памяти.
/// При изменении данных в админ-панели соответствующий ключ сбрасывается.
/// </summary>
public class SiteCache
{
    private const string SettingsKey = "settings";
    private const string ServicesKey = "services";
    private const string SlidesKey = "slides";
    private const string ReviewsKey = "featured-reviews";
    private const string TeamKey = "team";

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    private readonly IMemoryCache _cache;
    private readonly AppDbContext _db;

    public SiteCache(IMemoryCache cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public Task<SiteSettings> GetSettingsAsync() =>
        _cache.GetOrCreateAsync(SettingsKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return _db.SiteSettings.AsNoTracking().OrderBy(s => s.Id).FirstAsync();
        })!;

    public Task<List<ServiceItem>> GetServicesAsync() =>
        _cache.GetOrCreateAsync(ServicesKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return _db.Services.AsNoTracking().Where(s => s.IsActive)
                .OrderBy(s => s.SortOrder).ThenBy(s => s.Title).ToListAsync();
        })!;

    public Task<List<Slide>> GetSlidesAsync() =>
        _cache.GetOrCreateAsync(SlidesKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return _db.Slides.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.SortOrder).ToListAsync();
        })!;

    public Task<List<Review>> GetFeaturedReviewsAsync() =>
        _cache.GetOrCreateAsync(ReviewsKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return _db.Reviews.AsNoTracking()
                .Where(r => r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.IsFeatured).ThenByDescending(r => r.CreatedAt)
                .Take(3).ToListAsync();
        })!;

    public Task<List<TeamMember>> GetTeamAsync() =>
        _cache.GetOrCreateAsync(TeamKey, e =>
        {
            e.AbsoluteExpirationRelativeToNow = Ttl;
            return _db.TeamMembers.AsNoTracking().OrderBy(t => t.SortOrder).ToListAsync();
        })!;

    public void ResetSettings() => _cache.Remove(SettingsKey);
    public void ResetServices() => _cache.Remove(ServicesKey);
    public void ResetSlides() => _cache.Remove(SlidesKey);
    public void ResetReviews() => _cache.Remove(ReviewsKey);
    public void ResetTeam() => _cache.Remove(TeamKey);
}
