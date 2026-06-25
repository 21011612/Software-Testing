using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;

namespace QuanLyQuanCafe.Services;

public class AuthService
{
    private readonly TooruCoffeeDbContext _db;
    private readonly IHttpContextAccessor _http;

    public AuthService(TooruCoffeeDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public Task<(bool Success, string? Error)> DangNhapKhachHangAsync(DangNhapViewModel model) =>
        DangNhapAsync(model, "KhachHang");

    public Task<(bool Success, string? Error)> DangNhapQuanTriAsync(DangNhapViewModel model) =>
        DangNhapAsync(model, "Admin", "NhanVien");

    public async Task<(bool Success, string? Error)> DangNhapAsync(DangNhapViewModel model, params string[] allowedRoles)
    {
        var login = (model.TenDangNhap ?? "").Trim();
        var matKhau = model.MatKhau ?? "";

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(matKhau))
            return (false, "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.");

        var tk = await _db.TaiKhoans
            .Include(t => t.KhachHang)
            .Include(t => t.NhanVien)
            .FirstOrDefaultAsync(t => t.TenDangNhap.ToLower() == login.ToLower() && t.TrangThai);

        if (tk == null)
            return (false, "Tên đăng nhập hoặc mật khẩu không đúng. (Không tìm thấy tài khoản hoặc đã bị khóa)");

        if (!PasswordHelper.Verify(matKhau, tk.MatKhauHash))
            return (false, "Tên đăng nhập hoặc mật khẩu không đúng. Kiểm tra lại mật khẩu (phân biệt chữ hoa, ký tự đặc biệt).");

        if (allowedRoles.Length > 0 && !allowedRoles.Contains(tk.VaiTro))
        {
            return tk.VaiTro is "Admin" or "NhanVien"
                ? (false, "Tài khoản nhân viên/quản trị. Vui lòng đăng nhập tại cổng Quản trị.")
                : (false, "Tài khoản khách hàng. Vui lòng đăng nhập tại trang Đăng nhập khách.");
        }

        tk.LanDangNhapCuoi = DateTime.Now;
        await _db.SaveChangesAsync();

        var hoTen = tk.VaiTro switch
        {
            "KhachHang" => tk.KhachHang?.HoTen ?? model.TenDangNhap,
            "NhanVien" or "Admin" => tk.NhanVien?.HoTen ?? model.TenDangNhap,
            _ => model.TenDangNhap
        } ?? tk.TenDangNhap;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tk.MaTK.ToString()),
            new(ClaimTypes.Name, tk.TenDangNhap),
            new(ClaimTypes.Role, tk.VaiTro),
            new("HoTen", hoTen)
        };
        if (tk.MaKH.HasValue) claims.Add(new Claim("MaKH", tk.MaKH.Value.ToString()));
        if (tk.MaNV.HasValue) claims.Add(new Claim("MaNV", tk.MaNV.Value.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await _http.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7) });

        return (true, null);
    }

    public async Task DangXuatAsync()
    {
        await _http.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public int? GetMaKH()
    {
        var v = _http.HttpContext?.User.FindFirst("MaKH")?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    public int? GetMaNV()
    {
        var v = _http.HttpContext?.User.FindFirst("MaNV")?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    public string? GetVaiTro() => _http.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
}