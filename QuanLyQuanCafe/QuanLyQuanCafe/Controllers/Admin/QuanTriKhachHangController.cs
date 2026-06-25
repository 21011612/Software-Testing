using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriKhachHangController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriKhachHangController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.KhachHangs
            .OrderByDescending(k => k.DiemTichLuy)
            .ThenBy(k => k.HoTen)
            .ToListAsync();
        return View(list);
    }

    public IActionResult Them() => View(new KhachHangAdminViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(KhachHangAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _db.KhachHangs.AnyAsync(k => k.SDT == model.SDT.Trim()))
        {
            ModelState.AddModelError(nameof(model.SDT), "SĐT đã tồn tại.");
            return View(model);
        }

        _db.KhachHangs.Add(MapToEntity(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var k = await _db.KhachHangs.FindAsync(id);
        if (k == null) return NotFound();
        return View(MapToVm(k));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, KhachHangAdminViewModel model)
    {
        if (id != model.MaKH) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var k = await _db.KhachHangs.FindAsync(id);
        if (k == null) return NotFound();

        if (await _db.KhachHangs.AnyAsync(x => x.SDT == model.SDT.Trim() && x.MaKH != id))
        {
            ModelState.AddModelError(nameof(model.SDT), "SĐT đã được khách khác sử dụng.");
            return View(model);
        }

        ApplyToEntity(k, model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var k = await _db.KhachHangs.FindAsync(id);
        if (k == null) return NotFound();

        if (await _db.HoaDons.AnyAsync(h => h.MaKH == id) || await _db.TaiKhoans.AnyAsync(t => t.MaKH == id))
        {
            TempData["Error"] = "Không xóa được — khách đã có tài khoản hoặc hóa đơn.";
            return RedirectToAction(nameof(Index));
        }

        _db.KhachHangs.Remove(k);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa khách hàng.";
        return RedirectToAction(nameof(Index));
    }

    private static KhachHang MapToEntity(KhachHangAdminViewModel m) => new()
    {
        HoTen = m.HoTen.Trim(),
        SDT = m.SDT.Trim(),
        Email = m.Email,
        DiaChi = m.DiaChi,
        NgaySinh = m.NgaySinh,
        DiemTichLuy = m.DiemTichLuy,
        HangThanhVien = m.HangThanhVien,
        GhiChu = m.GhiChu,
        NgayTao = DateTime.Now
    };

    private static void ApplyToEntity(KhachHang k, KhachHangAdminViewModel m)
    {
        k.HoTen = m.HoTen.Trim();
        k.SDT = m.SDT.Trim();
        k.Email = m.Email;
        k.DiaChi = m.DiaChi;
        k.NgaySinh = m.NgaySinh;
        k.DiemTichLuy = m.DiemTichLuy;
        k.HangThanhVien = m.HangThanhVien;
        k.GhiChu = m.GhiChu;
    }

    private static KhachHangAdminViewModel MapToVm(KhachHang k) => new()
    {
        MaKH = k.MaKH,
        HoTen = k.HoTen,
        SDT = k.SDT,
        Email = k.Email,
        DiaChi = k.DiaChi,
        NgaySinh = k.NgaySinh,
        DiemTichLuy = k.DiemTichLuy,
        HangThanhVien = k.HangThanhVien,
        GhiChu = k.GhiChu
    };
}