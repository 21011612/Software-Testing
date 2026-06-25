using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriLichSuController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriLichSuController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? loai, int? maNv, DateTime? tuNgay, DateTime? denNgay)
    {
        var q = _db.LichSuHoatDongs.Include(l => l.NhanVien).AsQueryable();

        if (!string.IsNullOrWhiteSpace(loai))
            q = q.Where(l => l.LoaiHanhDong.Contains(loai));
        if (maNv.HasValue)
            q = q.Where(l => l.MaNV == maNv);
        if (tuNgay.HasValue)
            q = q.Where(l => l.ThoiGian >= tuNgay.Value.Date);
        if (denNgay.HasValue)
            q = q.Where(l => l.ThoiGian < denNgay.Value.Date.AddDays(1));

        ViewBag.Loai = loai;
        ViewBag.MaNv = maNv;
        ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
        ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
        ViewBag.NhanViens = new SelectList(
            await _db.NhanViens.OrderBy(n => n.HoTen).ToListAsync(),
            "MaNV", "HoTen", maNv);

        var list = await q.OrderByDescending(l => l.ThoiGian).Take(200).ToListAsync();
        return View(list);
    }
}