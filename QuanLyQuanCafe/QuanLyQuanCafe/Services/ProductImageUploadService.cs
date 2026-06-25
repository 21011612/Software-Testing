using System.Text.RegularExpressions;

namespace QuanLyQuanCafe.Services;

public class ProductImageUploadService
{
    private static readonly HashSet<string> AllowedExt = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;

    public ProductImageUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<(bool Success, string? RelativePath, string? Error)> SaveProductImageAsync(
        IFormFile? file,
        string productName)
    {
        if (file == null || file.Length == 0)
            return (false, null, null);

        if (file.Length > MaxFileSize)
            return (false, null, "Ảnh tối đa 5 MB.");

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(ext) || !AllowedExt.Contains(ext))
            return (false, null, "Chỉ chấp nhận JPG, PNG, WEBP hoặc GIF.");

        var imagesDir = Path.Combine(_env.WebRootPath, "images");
        Directory.CreateDirectory(imagesDir);

        var baseName = Slugify(productName);
        if (string.IsNullOrEmpty(baseName))
            baseName = "san_pham";

        var fileName = $"{baseName}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext.ToLowerInvariant()}";
        var fullPath = Path.Combine(imagesDir, fileName);

        await using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        return (true, $"images/{fileName}", null);
    }

    private static string Slugify(string text)
    {
        var s = text.Trim().ToLowerInvariant();
        s = Regex.Replace(s, @"[àáạảãâầấậẩẫăằắặẳẵ]", "a");
        s = Regex.Replace(s, @"[èéẹẻẽêềếệểễ]", "e");
        s = Regex.Replace(s, @"[ìíịỉĩ]", "i");
        s = Regex.Replace(s, @"[òóọỏõôồốộổỗơờớợởỡ]", "o");
        s = Regex.Replace(s, @"[ùúụủũưừứựửữ]", "u");
        s = Regex.Replace(s, @"[ỳýỵỷỹ]", "y");
        s = Regex.Replace(s, @"[đ]", "d");
        s = Regex.Replace(s, @"[^a-z0-9]+", "_");
        return s.Trim('_');
    }
}