using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriLoaiSanPhamController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriLoaiSanPhamController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.LoaiSanPhams.OrderBy(l => l.ThuTuHienThi).ThenBy(l => l.TenLoai).ToListAsync();
        return View(list);
    }

    public IActionResult Them() => View(new LoaiSanPhamAdminViewModel { ThuTuHienThi = 1 });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(LoaiSanPhamAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        _db.LoaiSanPhams.Add(MapToEntity(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm loại sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var l = await _db.LoaiSanPhams.FindAsync(id);
        if (l == null) return NotFound();
        return View(MapToVm(l));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, LoaiSanPhamAdminViewModel model)
    {
        if (id != model.MaLoai) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var l = await _db.LoaiSanPhams.FindAsync(id);
        if (l == null) return NotFound();

        ApplyToEntity(l, model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật loại sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var l = await _db.LoaiSanPhams.FindAsync(id);
        if (l == null) return NotFound();

        if (await _db.SanPhams.AnyAsync(s => s.MaLoai == id))
        {
            TempData["Error"] = "Không xóa được — loại đang có sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        _db.LoaiSanPhams.Remove(l);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa loại sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    private static LoaiSanPham MapToEntity(LoaiSanPhamAdminViewModel m) => new()
    {
        TenLoai = m.TenLoai.Trim(),
        MoTa = m.MoTa,
        HinhAnh = m.HinhAnh,
        ThuTuHienThi = m.ThuTuHienThi,
        TrangThai = m.TrangThai
    };

    private static void ApplyToEntity(LoaiSanPham l, LoaiSanPhamAdminViewModel m)
    {
        l.TenLoai = m.TenLoai.Trim();
        l.MoTa = m.MoTa;
        l.HinhAnh = m.HinhAnh;
        l.ThuTuHienThi = m.ThuTuHienThi;
        l.TrangThai = m.TrangThai;
    }

    private static LoaiSanPhamAdminViewModel MapToVm(LoaiSanPham l) => new()
    {
        MaLoai = l.MaLoai,
        TenLoai = l.TenLoai,
        MoTa = l.MoTa,
        HinhAnh = l.HinhAnh,
        ThuTuHienThi = l.ThuTuHienThi,
        TrangThai = l.TrangThai
    };
}