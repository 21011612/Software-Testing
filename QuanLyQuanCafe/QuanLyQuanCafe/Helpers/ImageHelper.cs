namespace QuanLyQuanCafe.Helpers;

public static class ImageHelper
{
    public static string ToUrl(string? hinhAnhDb)
    {
        if (string.IsNullOrWhiteSpace(hinhAnhDb))
            return "/images/Ca_phe_sua_da.jpg";
        var path = hinhAnhDb.Trim().Replace('\\', '/');
        if (!path.StartsWith('/'))
            path = "/" + path;
        return path;
    }
}