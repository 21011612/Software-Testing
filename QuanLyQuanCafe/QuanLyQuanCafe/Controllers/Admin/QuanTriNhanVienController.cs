using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriNhanVienController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriNhanVienController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.NhanViens.OrderBy(n => n.ChucVu).ThenBy(n => n.HoTen).ToListAsync();
        return View(list);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Them() => View(new NhanVienAdminViewModel());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(NhanVienAdminViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (await _db.NhanViens.AnyAsync(n => n.SDT == model.SDT.Trim()))
        {
            ModelState.AddModelError(nameof(model.SDT), "SĐT đã tồn tại.");
            return View(model);
        }

        _db.NhanViens.Add(MapToEntity(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm nhân viên.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Sua(int id)
    {
        var nv = await _db.NhanViens.FindAsync(id);
        if (nv == null) return NotFound();
        return View(MapToVm(nv));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, NhanVienAdminViewModel model)
    {
        if (id != model.MaNV) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var nv = await _db.NhanViens.FindAsync(id);
        if (nv == null) return NotFound();

        if (await _db.NhanViens.AnyAsync(x => x.SDT == model.SDT.Trim() && x.MaNV != id))
        {
            ModelState.AddModelError(nameof(model.SDT), "SĐT đã được nhân viên khác sử dụng.");
            return View(model);
        }

        ApplyToEntity(nv, model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật nhân viên.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var nv = await _db.NhanViens.FindAsync(id);
        if (nv == null) return NotFound();

        if (await _db.HoaDons.AnyAsync(h => h.MaNV == id) || await _db.TaiKhoans.AnyAsync(t => t.MaNV == id))
        {
            nv.TrangThai = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Nhân viên đã có phát sinh — đã chuyển trạng thái nghỉ.";
            return RedirectToAction(nameof(Index));
        }

        _db.NhanViens.Remove(nv);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa nhân viên.";
        return RedirectToAction(nameof(Index));
    }

    private static NhanVien MapToEntity(NhanVienAdminViewModel m) => new()
    {
        HoTen = m.HoTen.Trim(),
        NgaySinh = m.NgaySinh,
        GioiTinh = m.GioiTinh,
        SDT = m.SDT.Trim(),
        Email = m.Email,
        DiaChi = m.DiaChi,
        ChucVu = m.ChucVu.Trim(),
        NgayVaoLam = m.NgayVaoLam,
        LuongCoBan = m.LuongCoBan,
        TrangThai = m.TrangThai
    };

    private static void ApplyToEntity(NhanVien nv, NhanVienAdminViewModel m)
    {
        nv.HoTen = m.HoTen.Trim();
        nv.NgaySinh = m.NgaySinh;
        nv.GioiTinh = m.GioiTinh;
        nv.SDT = m.SDT.Trim();
        nv.Email = m.Email;
        nv.DiaChi = m.DiaChi;
        nv.ChucVu = m.ChucVu.Trim();
        nv.NgayVaoLam = m.NgayVaoLam;
        nv.LuongCoBan = m.LuongCoBan;
        nv.TrangThai = m.TrangThai;
    }

    private static NhanVienAdminViewModel MapToVm(NhanVien nv) => new()
    {
        MaNV = nv.MaNV,
        HoTen = nv.HoTen,
        NgaySinh = nv.NgaySinh,
        GioiTinh = nv.GioiTinh,
        SDT = nv.SDT,
        Email = nv.Email,
        DiaChi = nv.DiaChi,
        ChucVu = nv.ChucVu,
        NgayVaoLam = nv.NgayVaoLam,
        LuongCoBan = nv.LuongCoBan,
        TrangThai = nv.TrangThai
    };
}