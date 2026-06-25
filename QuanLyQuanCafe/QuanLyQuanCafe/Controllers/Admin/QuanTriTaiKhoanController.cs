using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class QuanTriTaiKhoanController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriTaiKhoanController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.TaiKhoans
            .Include(t => t.NhanVien)
            .Include(t => t.KhachHang)
            .OrderBy(t => t.VaiTro)
            .ThenBy(t => t.TenDangNhap)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadDropdownsAsync();
        return View(new TaiKhoanAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(TaiKhoanAdminViewModel model)
    {
        ValidateTaiKhoan(model, isNew: true);
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap.Trim()))
        {
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
            await LoadDropdownsAsync();
            return View(model);
        }

        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap.Trim(),
            MatKhauHash = PasswordHelper.Hash(model.MatKhauMoi!),
            VaiTro = model.VaiTro,
            MaNV = model.MaNV,
            MaKH = model.MaKH,
            TrangThai = model.TrangThai,
            NgayTao = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();
        await LoadDropdownsAsync();
        return View(MapToVm(tk));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, TaiKhoanAdminViewModel model)
    {
        if (id != model.MaTK) return BadRequest();
        ValidateTaiKhoan(model, isNew: false);
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();

        if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap.Trim() && t.MaTK != id))
        {
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã được sử dụng.");
            await LoadDropdownsAsync();
            return View(model);
        }

        tk.TenDangNhap = model.TenDangNhap.Trim();
        tk.VaiTro = model.VaiTro;
        tk.MaNV = model.MaNV;
        tk.MaKH = model.MaKH;
        tk.TrangThai = model.TrangThai;
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
            tk.MatKhauHash = PasswordHelper.Hash(model.MatKhauMoi);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var tk = await _db.TaiKhoans.FindAsync(id);
        if (tk == null) return NotFound();

        _db.TaiKhoans.Remove(tk);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa tài khoản.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidateTaiKhoan(TaiKhoanAdminViewModel model, bool isNew)
    {
        if (isNew && string.IsNullOrWhiteSpace(model.MatKhauMoi))
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Nhập mật khẩu cho tài khoản mới.");

        switch (model.VaiTro)
        {
            case "Admin":
                if (model.MaNV.HasValue || model.MaKH.HasValue)
                    ModelState.AddModelError("", "Admin: không gắn nhân viên hoặc khách hàng.");
                break;
            case "NhanVien":
                if (!model.MaNV.HasValue || model.MaKH.HasValue)
                    ModelState.AddModelError("", "Nhân viên: chọn MaNV, không chọn MaKH.");
                break;
            case "KhachHang":
                if (!model.MaKH.HasValue || model.MaNV.HasValue)
                    ModelState.AddModelError("", "Khách hàng: chọn MaKH, không chọn MaNV.");
                break;
            default:
                ModelState.AddModelError(nameof(model.VaiTro), "Vai trò không hợp lệ.");
                break;
        }
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.NhanViens = new SelectList(
            await _db.NhanViens.Where(n => n.TrangThai).OrderBy(n => n.HoTen).ToListAsync(),
            "MaNV", "HoTen");
        ViewBag.KhachHangs = new SelectList(
            await _db.KhachHangs.OrderBy(k => k.HoTen).ToListAsync(),
            "MaKH", "HoTen");
    }

    private static TaiKhoanAdminViewModel MapToVm(TaiKhoan tk) => new()
    {
        MaTK = tk.MaTK,
        TenDangNhap = tk.TenDangNhap,
        VaiTro = tk.VaiTro,
        MaNV = tk.MaNV,
        MaKH = tk.MaKH,
        TrangThai = tk.TrangThai
    };
}