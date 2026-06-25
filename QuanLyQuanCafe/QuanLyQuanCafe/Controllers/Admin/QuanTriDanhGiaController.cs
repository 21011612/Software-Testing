using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriDanhGiaController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriDanhGiaController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, bool? an)
    {
        var query = _db.DanhGiaBinhLuans
            .Include(d => d.SanPham)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            query = query.Where(d =>
                d.HoTenHienThi.Contains(kw) ||
                d.NoiDung.Contains(kw) ||
                (d.SanPham != null && d.SanPham.TenSP.Contains(kw)));
        }

        if (an == true)
            query = query.Where(d => !d.TrangThai);

        var list = await query
            .OrderByDescending(d => d.NgayTao)
            .Select(d => new QuanTriDanhGiaRowViewModel
            {
                MaDG = d.MaDG,
                MaSP = d.MaSP,
                TenSP = d.SanPham != null ? d.SanPham.TenSP : "",
                HoTenHienThi = d.HoTenHienThi,
                SoSao = d.SoSao,
                NoiDung = d.NoiDung,
                NgayTao = d.NgayTao,
                TrangThai = d.TrangThai
            })
            .ToListAsync();

        ViewBag.TuKhoa = q;
        ViewBag.LocAn = an;
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DoiTrangThai(int id)
    {
        var dg = await _db.DanhGiaBinhLuans.FindAsync(id);
        if (dg == null) return NotFound();
        dg.TrangThai = !dg.TrangThai;
        await _db.SaveChangesAsync();
        TempData["Success"] = dg.TrangThai ? "Đã hiện bình luận." : "Đã ẩn bình luận.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Xoa(int id)
    {
        var dg = await _db.DanhGiaBinhLuans.FindAsync(id);
        if (dg == null) return NotFound();
        _db.DanhGiaBinhLuans.Remove(dg);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa bình luận.";
        return RedirectToAction(nameof(Index));
    }
}