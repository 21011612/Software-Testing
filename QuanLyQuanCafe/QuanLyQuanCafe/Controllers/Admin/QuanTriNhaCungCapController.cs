using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriNhaCungCapController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriNhaCungCapController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.NhaCungCaps.OrderBy(n => n.TenNCC).ToListAsync();
        return View(list);
    }

    public IActionResult Them() => View(new NhaCungCapAdminViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(NhaCungCapAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.NhaCungCaps.Add(MapToEntity(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm nhà cung cấp.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var n = await _db.NhaCungCaps.FindAsync(id);
        if (n == null) return NotFound();
        return View(MapToVm(n));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, NhaCungCapAdminViewModel model)
    {
        if (id != model.MaNCC) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var n = await _db.NhaCungCaps.FindAsync(id);
        if (n == null) return NotFound();

        ApplyToEntity(n, model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật nhà cung cấp.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var n = await _db.NhaCungCaps.FindAsync(id);
        if (n == null) return NotFound();

        if (await _db.PhieuNhaps.AnyAsync(p => p.MaNCC == id) || await _db.NguyenLieus.AnyAsync(nl => nl.MaNCC == id))
        {
            TempData["Error"] = "Không xóa được — NCC đã có phiếu nhập hoặc nguyên liệu.";
            return RedirectToAction(nameof(Index));
        }

        _db.NhaCungCaps.Remove(n);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa nhà cung cấp.";
        return RedirectToAction(nameof(Index));
    }

    private static NhaCungCap MapToEntity(NhaCungCapAdminViewModel m) => new()
    {
        TenNCC = m.TenNCC.Trim(),
        SDT = m.SDT,
        Email = m.Email,
        DiaChi = m.DiaChi,
        NguoiLienHe = m.NguoiLienHe,
        TrangThai = m.TrangThai
    };

    private static void ApplyToEntity(NhaCungCap n, NhaCungCapAdminViewModel m)
    {
        n.TenNCC = m.TenNCC.Trim();
        n.SDT = m.SDT;
        n.Email = m.Email;
        n.DiaChi = m.DiaChi;
        n.NguoiLienHe = m.NguoiLienHe;
        n.TrangThai = m.TrangThai;
    }

    private static NhaCungCapAdminViewModel MapToVm(NhaCungCap n) => new()
    {
        MaNCC = n.MaNCC,
        TenNCC = n.TenNCC,
        SDT = n.SDT,
        Email = n.Email,
        DiaChi = n.DiaChi,
        NguoiLienHe = n.NguoiLienHe,
        TrangThai = n.TrangThai
    };
}