using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriPhieuNhapController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly AuthService _auth;

    public QuanTriPhieuNhapController(TooruCoffeeDbContext db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.PhieuNhaps
            .Include(p => p.NhaCungCap)
            .Include(p => p.NhanVien)
            .OrderByDescending(p => p.NgayNhap)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadDropdownsAsync();
        var maNv = _auth.GetMaNV();
        return View(new PhieuNhapAdminViewModel { MaNV = maNv ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(PhieuNhapAdminViewModel model)
    {
        var maNv = model.MaNV > 0 ? model.MaNV : _auth.GetMaNV();
        if (!maNv.HasValue || maNv <= 0)
        {
            ModelState.AddModelError(nameof(model.MaNV), "Không xác định được nhân viên.");
            await LoadDropdownsAsync();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return View(model);
        }

        var pn = new PhieuNhap
        {
            MaNCC = model.MaNCC,
            MaNV = maNv.Value,
            NgayNhap = DateTime.Now,
            TongTien = 0,
            GhiChu = model.GhiChu
        };
        _db.PhieuNhaps.Add(pn);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo phiếu nhập — thêm chi tiết bên dưới.";
        return RedirectToAction(nameof(ChiTiet), new { id = pn.MaPN });
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var pn = await _db.PhieuNhaps
            .Include(p => p.NhaCungCap)
            .Include(p => p.NhanVien)
            .Include(p => p.ChiTietPhieuNhaps).ThenInclude(c => c.NguyenLieu)
            .FirstOrDefaultAsync(p => p.MaPN == id);
        if (pn == null) return NotFound();

        await LoadNguyenLieuAsync();
        ViewBag.Line = new ChiTietPhieuNhapLineViewModel();
        return View(pn);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemDong(int id, ChiTietPhieuNhapLineViewModel line)
    {
        var pn = await _db.PhieuNhaps.FindAsync(id);
        if (pn == null) return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu dòng nhập không hợp lệ.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        _db.ChiTietPhieuNhaps.Add(new ChiTietPhieuNhap
        {
            MaPN = id,
            MaNL = line.MaNL,
            SoLuong = line.SoLuong,
            DonGia = line.DonGia
        });
        await _db.SaveChangesAsync();

        await CapNhatTongTienAsync(id);
        TempData["Success"] = "Đã thêm dòng nhập.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaDong(int id, int maCt)
    {
        var ct = await _db.ChiTietPhieuNhaps.FirstOrDefaultAsync(c => c.MaCTPN == maCt && c.MaPN == id);
        if (ct == null) return NotFound();

        _db.ChiTietPhieuNhaps.Remove(ct);
        await _db.SaveChangesAsync();
        await CapNhatTongTienAsync(id);
        TempData["Success"] = "Đã xóa dòng nhập.";
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    private async Task CapNhatTongTienAsync(int maPn)
    {
        var tong = await _db.ChiTietPhieuNhaps.Where(c => c.MaPN == maPn).SumAsync(c => c.ThanhTien);
        var pn = await _db.PhieuNhaps.FindAsync(maPn);
        if (pn != null)
        {
            pn.TongTien = tong;
            await _db.SaveChangesAsync();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        ViewBag.NhaCungCaps = new SelectList(
            await _db.NhaCungCaps.Where(n => n.TrangThai).OrderBy(n => n.TenNCC).ToListAsync(),
            "MaNCC", "TenNCC");
        ViewBag.NhanViens = new SelectList(
            await _db.NhanViens.Where(n => n.TrangThai).OrderBy(n => n.HoTen).ToListAsync(),
            "MaNV", "HoTen");
    }

    private async Task LoadNguyenLieuAsync() =>
        ViewBag.NguyenLieus = new SelectList(
            await _db.NguyenLieus.OrderBy(n => n.TenNL).ToListAsync(),
            "MaNL", "TenNL");
}