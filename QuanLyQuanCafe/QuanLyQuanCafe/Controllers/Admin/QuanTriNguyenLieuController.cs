using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriNguyenLieuController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriNguyenLieuController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.NguyenLieus.Include(n => n.NhaCungCap).OrderBy(n => n.TenNL).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadNccAsync();
        return View(new NguyenLieuAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(NguyenLieuAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadNccAsync();
            return View(model);
        }

        _db.NguyenLieus.Add(new NguyenLieu
        {
            TenNL = model.TenNL.Trim(),
            DonViTinh = model.DonViTinh,
            SoLuongTon = model.SoLuongTon,
            GiaNhapTrungBinh = model.GiaNhapTrungBinh,
            MaNCC = model.MaNCC,
            NgayCapNhat = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm nguyên liệu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var nl = await _db.NguyenLieus.FindAsync(id);
        if (nl == null) return NotFound();
        await LoadNccAsync();
        return View(MapToVm(nl));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, NguyenLieuAdminViewModel model)
    {
        if (id != model.MaNL) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadNccAsync();
            return View(model);
        }

        var nl = await _db.NguyenLieus.FindAsync(id);
        if (nl == null) return NotFound();

        nl.TenNL = model.TenNL.Trim();
        nl.DonViTinh = model.DonViTinh;
        nl.SoLuongTon = model.SoLuongTon;
        nl.GiaNhapTrungBinh = model.GiaNhapTrungBinh;
        nl.MaNCC = model.MaNCC;
        nl.NgayCapNhat = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật nguyên liệu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var nl = await _db.NguyenLieus.FindAsync(id);
        if (nl == null) return NotFound();

        if (await _db.ChiTietPhieuNhaps.AnyAsync(c => c.MaNL == id) || await _db.CongThucs.AnyAsync(c => c.MaNL == id))
        {
            TempData["Error"] = "Không xóa được — nguyên liệu đã dùng trong kho/công thức.";
            return RedirectToAction(nameof(Index));
        }

        _db.NguyenLieus.Remove(nl);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa nguyên liệu.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadNccAsync() =>
        ViewBag.NhaCungCaps = new SelectList(
            await _db.NhaCungCaps.Where(n => n.TrangThai).OrderBy(n => n.TenNCC).ToListAsync(),
            "MaNCC", "TenNCC");

    private static NguyenLieuAdminViewModel MapToVm(NguyenLieu nl) => new()
    {
        MaNL = nl.MaNL,
        TenNL = nl.TenNL,
        DonViTinh = nl.DonViTinh,
        SoLuongTon = nl.SoLuongTon,
        GiaNhapTrungBinh = nl.GiaNhapTrungBinh,
        MaNCC = nl.MaNCC
    };
}