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
public class QuanTriDatBanController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriDatBanController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        var list = await _db.DatBans
            .Include(d => d.Ban)
            .Include(d => d.KhachHang)
            .OrderByDescending(d => d.NgayDat)
            .ThenBy(d => d.GioBatDau)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadSelectListsAsync();
        return View(new DatBanAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(DatBanAdminViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(model);
        }
        if (!TryParseGio(model, out var gioBat, out var gioKet))
        {
            await LoadSelectListsAsync();
            return View(model);
        }

        _db.DatBans.Add(new DatBan
        {
            MaBan = model.MaBan,
            MaKH = model.MaKH > 0 ? model.MaKH : null,
            TenKhach = model.TenKhach.Trim(),
            SDT = model.SDT.Trim(),
            NgayDat = model.NgayDat.Date,
            GioBatDau = gioBat,
            GioKetThuc = gioKet,
            SoNguoi = model.SoNguoi,
            TrangThai = model.TrangThai,
            GhiChu = model.GhiChu,
            NgayTao = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm đặt bàn.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var d = await _db.DatBans.FindAsync(id);
        if (d == null) return NotFound();
        await LoadSelectListsAsync();
        return View(MapToVm(d));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, DatBanAdminViewModel model)
    {
        if (id != model.MaDatBan) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return View(model);
        }
        if (!TryParseGio(model, out var gioBat, out var gioKet))
        {
            await LoadSelectListsAsync();
            return View(model);
        }

        var d = await _db.DatBans.FindAsync(id);
        if (d == null) return NotFound();

        d.MaBan = model.MaBan;
        d.MaKH = model.MaKH > 0 ? model.MaKH : null;
        d.TenKhach = model.TenKhach.Trim();
        d.SDT = model.SDT.Trim();
        d.NgayDat = model.NgayDat.Date;
        d.GioBatDau = gioBat;
        d.GioKetThuc = gioKet;
        d.SoNguoi = model.SoNguoi;
        d.TrangThai = model.TrangThai;
        d.GhiChu = model.GhiChu;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật đặt bàn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var d = await _db.DatBans.FindAsync(id);
        if (d == null) return NotFound();

        _db.DatBans.Remove(d);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa đặt bàn.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectListsAsync()
    {
        ViewBag.Bans = new SelectList(
            await _db.Bans.OrderBy(b => b.KhuVuc).ThenBy(b => b.TenBan).ToListAsync(),
            "MaBan", "TenBan");
        ViewBag.Khachs = new SelectList(
            await _db.KhachHangs.OrderBy(k => k.HoTen).ToListAsync(),
            "MaKH", "HoTen");
    }

    private bool TryParseGio(DatBanAdminViewModel model, out TimeSpan gioBat, out TimeSpan? gioKet)
    {
        gioBat = default;
        gioKet = null;
        if (!TimeSpan.TryParse(model.GioBatDau, out gioBat))
        {
            ModelState.AddModelError(nameof(model.GioBatDau), "Giờ bắt đầu không hợp lệ (HH:mm).");
            return false;
        }
        if (!string.IsNullOrWhiteSpace(model.GioKetThuc))
        {
            if (TimeSpan.TryParse(model.GioKetThuc, out var ket))
                gioKet = ket;
            else
                ModelState.AddModelError(nameof(model.GioKetThuc), "Giờ kết thúc không hợp lệ (HH:mm).");
        }
        return true;
    }

    private static DatBanAdminViewModel MapToVm(DatBan d) => new()
    {
        MaDatBan = d.MaDatBan,
        MaBan = d.MaBan,
        MaKH = d.MaKH,
        TenKhach = d.TenKhach,
        SDT = d.SDT,
        NgayDat = d.NgayDat,
        GioBatDau = d.GioBatDau.ToString(@"hh\:mm"),
        GioKetThuc = d.GioKetThuc?.ToString(@"hh\:mm"),
        SoNguoi = d.SoNguoi,
        TrangThai = d.TrangThai,
        GhiChu = d.GhiChu
    };
}