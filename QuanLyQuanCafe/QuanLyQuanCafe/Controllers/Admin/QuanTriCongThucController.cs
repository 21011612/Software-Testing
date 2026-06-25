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
public class QuanTriCongThucController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriCongThucController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.CongThucs
            .Include(c => c.SanPham)
            .Include(c => c.NguyenLieu)
            .OrderBy(c => c.MaSP)
            .ThenBy(c => c.MaNL)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadDropdownsAsync();
        return View(new CongThucAdminViewModel { SoLuongCan = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(CongThucAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        if (await _db.CongThucs.AnyAsync(c => c.MaSP == model.MaSP && c.MaNL == model.MaNL))
        {
            ModelState.AddModelError("", "Công thức SP–NL đã tồn tại.");
            await LoadDropdownsAsync();
            return View(model);
        }

        _db.CongThucs.Add(new CongThuc
        {
            MaSP = model.MaSP,
            MaNL = model.MaNL,
            SoLuongCan = model.SoLuongCan
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm công thức.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var ct = await _db.CongThucs.FindAsync(id);
        if (ct == null) return NotFound();
        await LoadDropdownsAsync();
        return View(MapToVm(ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, CongThucAdminViewModel model)
    {
        if (id != model.MaCongThuc) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var ct = await _db.CongThucs.FindAsync(id);
        if (ct == null) return NotFound();

        if (await _db.CongThucs.AnyAsync(c => c.MaSP == model.MaSP && c.MaNL == model.MaNL && c.MaCongThuc != id))
        {
            ModelState.AddModelError("", "Công thức SP–NL đã tồn tại.");
            await LoadDropdownsAsync();
            return View(model);
        }

        ct.MaSP = model.MaSP;
        ct.MaNL = model.MaNL;
        ct.SoLuongCan = model.SoLuongCan;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật công thức.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var ct = await _db.CongThucs.FindAsync(id);
        if (ct == null) return NotFound();

        _db.CongThucs.Remove(ct);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa công thức.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.SanPhams = new SelectList(
            await _db.SanPhams.Where(s => s.TrangThai).OrderBy(s => s.TenSP).ToListAsync(),
            "MaSP", "TenSP");
        ViewBag.NguyenLieus = new SelectList(
            await _db.NguyenLieus.OrderBy(n => n.TenNL).ToListAsync(),
            "MaNL", "TenNL");
    }

    private static CongThucAdminViewModel MapToVm(CongThuc c) => new()
    {
        MaCongThuc = c.MaCongThuc,
        MaSP = c.MaSP,
        MaNL = c.MaNL,
        SoLuongCan = c.SoLuongCan
    };
}