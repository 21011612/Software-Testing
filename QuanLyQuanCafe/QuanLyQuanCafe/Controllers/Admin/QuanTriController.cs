using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

public class QuanTriController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly AuthService _auth;
    private readonly StatisticsService _stats;

    public QuanTriController(TooruCoffeeDbContext db, AuthService auth, StatisticsService stats)
    {
        _db = db;
        _auth = auth;
        _stats = stats;
    }

    [AllowAnonymous]
    public IActionResult DangNhap(string? returnUrl = null)
    {
        if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
            return RedirectToAction(nameof(Index));
        return View(new DangNhapViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, err) = await _auth.DangNhapQuanTriAsync(model);
        if (!ok)
        {
            ModelState.AddModelError("", err ?? "Đăng nhập thất bại.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin,NhanVien")]
    public async Task<IActionResult> DangXuat()
    {
        await _auth.DangXuatAsync();
        return RedirectToAction(nameof(DangNhap));
    }

    [Authorize(Roles = "Admin,NhanVien")]
    public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
    {
        ViewBag.DonGanDay = await _db.HoaDons
            .Include(h => h.NhanVien)
            .OrderByDescending(h => h.NgayLap)
            .Take(10)
            .ToListAsync();
        return View(await _stats.LayDashboardAsync(tuNgay, denNgay));
    }

    [Authorize(Roles = "Admin,NhanVien")]
    public IActionResult ThongKe(DateTime? tuNgay, DateTime? denNgay) =>
        RedirectToAction(nameof(Index), new { tuNgay, denNgay });

    [Authorize(Roles = "Admin,NhanVien")]
    public IActionResult HoaDon(string? trangThai, string? loaiDon, DateTime? tuNgay, DateTime? denNgay) =>
        RedirectToAction("Index", "QuanTriHoaDon", new { trangThai, loaiDon, tuNgay, denNgay });

    [Authorize(Roles = "Admin,NhanVien")]
    public async Task<IActionResult> Ban()
    {
        var bans = await _db.Bans.OrderBy(b => b.KhuVuc).ThenBy(b => b.TenBan).ToListAsync();
        return View(bans);
    }

    [Authorize(Roles = "Admin,NhanVien")]
    public IActionResult ThemBan() => View(new BanAdminViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin,NhanVien")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemBan(BanAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _db.Bans.AnyAsync(b => b.TenBan == model.TenBan.Trim()))
        {
            ModelState.AddModelError(nameof(model.TenBan), "Tên bàn đã tồn tại.");
            return View(model);
        }
        _db.Bans.Add(new Ban
        {
            TenBan = model.TenBan.Trim(),
            KhuVuc = model.KhuVuc,
            SoCho = model.SoCho,
            TrangThai = model.TrangThai,
            GhiChu = model.GhiChu
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm bàn mới.";
        return RedirectToAction(nameof(Ban));
    }

    [Authorize(Roles = "Admin,NhanVien")]
    public async Task<IActionResult> SuaBan(int id)
    {
        var b = await _db.Bans.FindAsync(id);
        if (b == null) return NotFound();
        return View(new BanAdminViewModel
        {
            MaBan = b.MaBan,
            TenBan = b.TenBan,
            KhuVuc = b.KhuVuc,
            SoCho = b.SoCho,
            TrangThai = b.TrangThai,
            GhiChu = b.GhiChu
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,NhanVien")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuaBan(int id, BanAdminViewModel model)
    {
        if (id != model.MaBan) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var b = await _db.Bans.FindAsync(id);
        if (b == null) return NotFound();

        var trungTen = await _db.Bans.AnyAsync(x => x.TenBan == model.TenBan.Trim() && x.MaBan != id);
        if (trungTen)
        {
            ModelState.AddModelError(nameof(model.TenBan), "Tên bàn đã tồn tại.");
            return View(model);
        }

        b.TenBan = model.TenBan.Trim();
        b.KhuVuc = model.KhuVuc;
        b.SoCho = model.SoCho;
        b.TrangThai = model.TrangThai;
        b.GhiChu = model.GhiChu;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật bàn.";
        return RedirectToAction(nameof(Ban));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,NhanVien")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaBan(int id)
    {
        var b = await _db.Bans.FindAsync(id);
        if (b == null) return NotFound();

        var coHd = await _db.HoaDons.AnyAsync(h => h.MaBan == id);
        if (coHd)
        {
            TempData["Error"] = "Không xóa được — bàn đã có hóa đơn liên quan.";
            return RedirectToAction(nameof(Ban));
        }

        _db.Bans.Remove(b);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bàn.";
        return RedirectToAction(nameof(Ban));
    }
}