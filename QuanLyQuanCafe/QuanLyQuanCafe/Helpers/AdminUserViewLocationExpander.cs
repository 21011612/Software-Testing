using Microsoft.AspNetCore.Mvc.Razor;

namespace QuanLyQuanCafe.Helpers;

/// <summary>
/// Ưu tiên view theo vùng Admin/User, vẫn fallback về Views/Shared và đường dẫn mặc định.
/// </summary>
public class AdminUserViewLocationExpander : IViewLocationExpander
{
    private const string ZoneKey = "view_zone";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var controller = context.ControllerName ?? "";
        context.Values[ZoneKey] = controller.StartsWith("QuanTri", StringComparison.Ordinal)
            ? "Admin"
            : "User";
    }

    public IEnumerable<string> ExpandViewLocations(
        ViewLocationExpanderContext context,
        IEnumerable<string> viewLocations)
    {
        if (!context.Values.TryGetValue(ZoneKey, out var zone) || string.IsNullOrEmpty(zone))
            return viewLocations;

        var prefix = $"/Views/{zone}/";
        var zoneFirst = new List<string>();

        foreach (var location in viewLocations)
        {
            if (location.StartsWith("/Views/Shared/", StringComparison.Ordinal))
            {
                zoneFirst.Add($"{prefix}Shared/{{0}}.cshtml");
            }
            else if (location.StartsWith("/Views/", StringComparison.Ordinal)
                     && !location.StartsWith("/Views/Shared/", StringComparison.Ordinal))
            {
                zoneFirst.Add(location.Replace("/Views/", prefix, StringComparison.Ordinal));
            }
        }

        return zoneFirst.Concat(viewLocations);
    }
}