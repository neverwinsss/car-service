using System.Diagnostics;
using car_service.Models;
using car_service.Models.ViewModels;
using car_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace car_service.Controllers;

public class HomeController : Controller
{
    private readonly SiteCache _cache;

    public HomeController(SiteCache cache) => _cache = cache;

    public async Task<IActionResult> Index()
    {
        var services = await _cache.GetServicesAsync();
        var vm = new HomeViewModel
        {
            Slides = await _cache.GetSlidesAsync(),
            Services = services.Where(s => s.IsFeatured).Take(6).ToList(),
            Reviews = await _cache.GetFeaturedReviewsAsync(),
            Booking = await BookingController.PrefillAsync(User, HttpContext, services, compact: true)
        };
        if (vm.Services.Count == 0) vm.Services = services.Take(6).ToList();
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    [Route("Home/Status/{code:int}")]
    public IActionResult Status(int code)
    {
        Response.StatusCode = code;
        ViewData["Code"] = code;
        return View();
    }
}
