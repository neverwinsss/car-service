using Microsoft.AspNetCore.Html;

namespace car_service.Services;

/// <summary>Небольшой набор линейных SVG-иконок (встраиваются в HTML, без внешних запросов).</summary>
public static class Icons
{
    private static readonly Dictionary<string, string> Paths = new()
    {
        ["phone"] = "<path d='M22 16.9v3a2 2 0 0 1-2.2 2 19.8 19.8 0 0 1-8.6-3.1 19.5 19.5 0 0 1-6-6A19.8 19.8 0 0 1 2.1 4.2 2 2 0 0 1 4.1 2h3a2 2 0 0 1 2 1.7c.1.9.4 1.8.7 2.7a2 2 0 0 1-.5 2.1L8 9.8a16 16 0 0 0 6 6l1.3-1.3a2 2 0 0 1 2.1-.4c.9.3 1.8.6 2.7.7a2 2 0 0 1 1.7 2z'/>",
        ["wrench"] = "<path d='M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.8-3.8a6 6 0 0 1-7.9 7.9l-6.9 6.9a2.1 2.1 0 0 1-3-3l6.9-6.9a6 6 0 0 1 7.9-7.9z'/>",
        ["engine"] = "<path d='M4 10h2V8h4V6h4v2h3l2 3h2v6h-2l-2 2H8l-2-2H4z'/><path d='M2 12v4'/>",
        ["oil"] = "<path d='M12 3s6 6.5 6 11a6 6 0 0 1-12 0c0-4.5 6-11 6-11z'/><path d='M9.5 15a2.5 2.5 0 0 0 2.5 2.5'/>",
        ["tire"] = "<circle cx='12' cy='12' r='9'/><circle cx='12' cy='12' r='3.5'/><path d='M12 3v5.5M12 15.5V21M3 12h5.5M15.5 12H21'/>",
        ["brake"] = "<circle cx='12' cy='12' r='9'/><circle cx='12' cy='12' r='2'/><path d='M5.6 7.5a8 8 0 0 0 0 9'/><path d='M18.4 7.5a8 8 0 0 1 0 9'/>",
        ["battery"] = "<rect x='3' y='7' width='18' height='13' rx='2'/><path d='M7 7V4h3v3M14 7V4h3v3M7 13.5h4M15 11.5v4M13 13.5h4'/>",
        ["diag"] = "<rect x='2' y='4' width='20' height='13' rx='2'/><path d='M8 21h8M12 17v4'/><path d='M6 11h3l1.5-3 3 6 1.5-3h3'/>",
        ["ac"] = "<path d='M12 2v20M4.9 4.9l14.2 14.2M2 12h20M4.9 19.1 19.1 4.9'/><path d='m9 4 3 2 3-2M9 20l3-2 3 2'/>",
        ["body"] = "<path d='M5 17H3v-5l2-5h11l3 5h2v5h-2'/><circle cx='7.5' cy='17' r='2'/><circle cx='16.5' cy='17' r='2'/><path d='M9.5 17h5M5 12h14'/>",
        ["suspension"] = "<path d='M12 2v3M12 19v3'/><path d='M8 5h8M8 19h8'/><path d='M9 7l6 2-6 2 6 2-6 2 6 2'/>",
        ["star"] = "<path d='m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.8L12 17.8 5.8 21l1.2-6.8-5-4.9 6.9-1z'/>",
        ["check"] = "<path d='M20 6 9 17l-5-5'/>",
        ["clock"] = "<circle cx='12' cy='12' r='9'/><path d='M12 7v5l3 2'/>",
        ["map"] = "<path d='M12 21s-7-6.2-7-12a7 7 0 0 1 14 0c0 5.8-7 12-7 12z'/><circle cx='12' cy='9' r='2.5'/>",
        ["mail"] = "<rect x='2' y='4' width='20' height='16' rx='2'/><path d='m22 7-10 6L2 7'/>",
        ["user"] = "<circle cx='12' cy='8' r='4'/><path d='M4 21a8 8 0 0 1 16 0'/>",
        ["menu"] = "<path d='M3 6h18M3 12h18M3 18h18'/>",
        ["close"] = "<path d='M18 6 6 18M6 6l12 12'/>",
        ["shield"] = "<path d='M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z'/><path d='m9 12 2 2 4-4'/>",
        ["award"] = "<circle cx='12' cy='9' r='6'/><path d='M8.2 13.7 7 22l5-3 5 3-1.2-8.3'/>",
        ["car"] = "<path d='M5 17H3v-5l2-5h11l3 5h2v5h-2'/><circle cx='7.5' cy='17' r='2'/><circle cx='16.5' cy='17' r='2'/><path d='M9.5 17h5'/>",
        ["calendar"] = "<rect x='3' y='4' width='18' height='18' rx='2'/><path d='M16 2v4M8 2v4M3 10h18'/>",
        ["left"] = "<path d='m15 18-6-6 6-6'/>",
        ["right"] = "<path d='m9 18 6-6-6-6'/>",
        ["bell"] = "<path d='M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9'/><path d='M10.3 21a1.9 1.9 0 0 0 3.4 0'/>",
        ["chat"] = "<path d='M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z'/>",
        ["settings"] = "<circle cx='12' cy='12' r='3'/><path d='M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z'/>",
        ["grid"] = "<rect x='3' y='3' width='7' height='7'/><rect x='14' y='3' width='7' height='7'/><rect x='14' y='14' width='7' height='7'/><rect x='3' y='14' width='7' height='7'/>",
        ["image"] = "<rect x='3' y='3' width='18' height='18' rx='2'/><circle cx='8.5' cy='8.5' r='1.5'/><path d='m21 15-5-5L5 21'/>",
        ["users"] = "<circle cx='9' cy='8' r='4'/><path d='M1 21a8 8 0 0 1 16 0'/><path d='M16 4a4 4 0 0 1 0 8M23 21a8 8 0 0 0-5-7.4'/>",
        ["logout"] = "<path d='M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9'/>",
        ["home"] = "<path d='m3 10 9-7 9 7v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z'/><path d='M9 22V12h6v10'/>",
        ["send"] = "<path d='M22 2 11 13M22 2l-7 20-4-9-9-4z'/>",
        ["vk"] = "<path d='M3 7c.2 5.6 3 9.7 8 9.7h.3v-3.2c1.8.2 3.2 1.5 3.7 3.2H17c-.6-2.3-2.3-3.6-3.3-4.1 1-.6 2.5-2 2.8-4.6h-2.3c-.4 1.5-1.7 3-3 3.2V7H9v5.6C7.6 12.3 6 10.7 5.9 7z'/>",
        ["telegram"] = "<path d='M21.5 4.5 2.5 11.8l6 2.2 2.3 6.5 3.4-4 5 3.7z'/><path d='m8.5 14 10-7.5'/>",
        ["whatsapp"] = "<path d='M3 21l1.7-4.9A8.5 8.5 0 1 1 8 19.6z'/><path d='M9 9.5c.3 2.4 2.4 4.6 5 5l1-1.3-1.8-1-.8.8c-.9-.4-1.6-1.1-2-2l.8-.8-1-1.8z'/>"
    };

    public static IHtmlContent Svg(string name, int size = 20, string? cls = null)
    {
        var path = Paths.GetValueOrDefault(name) ?? Paths["wrench"];
        var c = cls == null ? "icon" : "icon " + cls;
        return new HtmlString(
            $"<svg class='{c}' width='{size}' height='{size}' viewBox='0 0 24 24' fill='none' stroke='currentColor' stroke-width='1.8' stroke-linecap='round' stroke-linejoin='round' aria-hidden='true'>{path}</svg>");
    }
}
