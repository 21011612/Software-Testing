using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriKhuyenMaiController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriKhuyenMaiController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.KhuyenMais.OrderByDescending(k => k.NgayBatDau).ToListAsync();
        return View(list);
    }

    public IActionResult Them() => View(new KhuyenMaiAdminViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(KhuyenMaiAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.NgayKetThuc < model.NgayBatDau)
            ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");
        if (!ModelState.IsValid) return View(model);

        _db.KhuyenMais.Add(MapToEntity(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm khuyến mãi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var km = await _db.KhuyenMais.FindAsync(id);
        if (km == null) return NotFound();
        return View(MapToVm(km));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, KhuyenMaiAdminViewModel model)
    {
        if (id != model.MaKM) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        if (model.NgayKetThuc < model.NgayBatDau)
            ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");
        if (!ModelState.IsValid) return View(model);

        var km = await _db.KhuyenMais.FindAsync(id);
        if (km == null) return NotFound();

        ApplyToEntity(km, model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật khuyến mãi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var km = await _db.KhuyenMais.FindAsync(id);
        if (km == null) return NotFound();

        if (await _db.HoaDons.AnyAsync(h => h.MaKM == id))
        {
            km.TrangThai = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "KM đã dùng trong hóa đơn — đã tắt thay vì xóa.";
            return RedirectToAction(nameof(Index));
        }

        _db.KhuyenMais.Remove(km);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa khuyến mãi.";
        return RedirectToAction(nameof(Index));
    }

    private static KhuyenMai MapToEntity(KhuyenMaiAdminViewModel m) => new()
    {
        TenKM = m.TenKM.Trim(),
        MoTa = m.MoTa,
        LoaiGiam = m.LoaiGiam,
        GiaTriGiam = m.GiaTriGiam,
        DieuKienToiThieu = m.DieuKienToiThieu,
        NgayBatDau = m.NgayBatDau.Date,
        NgayKetThuc = m.NgayKetThuc.Date,
        SoLuongToiDa = m.SoLuongToiDa,
        DaSuDung = 0,
        TrangThai = m.TrangThai
    };

    private static void ApplyToEntity(KhuyenMai km, KhuyenMaiAdminViewModel m)
    {
        km.TenKM = m.TenKM.Trim();
        km.MoTa = m.MoTa;
        km.LoaiGiam = m.LoaiGiam;
        km.GiaTriGiam = m.GiaTriGiam;
        km.DieuKienToiThieu = m.DieuKienToiThieu;
        km.NgayBatDau = m.NgayBatDau.Date;
        km.NgayKetThuc = m.NgayKetThuc.Date;
        km.SoLuongToiDa = m.SoLuongToiDa;
        km.TrangThai = m.TrangThai;
    }

    private static KhuyenMaiAdminViewModel MapToVm(KhuyenMai km) => new()
    {
        MaKM = km.MaKM,
        TenKM = km.TenKM,
        MoTa = km.MoTa,
        LoaiGiam = km.LoaiGiam,
        GiaTriGiam = km.GiaTriGiam,
        DieuKienToiThieu = km.DieuKienToiThieu,
        NgayBatDau = km.NgayBatDau,
        NgayKetThuc = km.NgayKetThuc,
        SoLuongToiDa = km.SoLuongToiDa,
        TrangThai = km.TrangThai
    };
}