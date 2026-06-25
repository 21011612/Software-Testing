using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriThanhToanController : Controller
{
    private readonly TooruCoffeeDbContext _db;

    public QuanTriThanhToanController(TooruCoffeeDbContext db) => _db = db;

    public async Task<IActionResult> Index(
        string? phuongThuc,
        string? trangThai,
        DateTime? tuNgay,
        DateTime? denNgay,
        string? q)
    {
        var query = _db.ThanhToans
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.Ban)
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.KhachHang)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(phuongThuc))
            query = query.Where(t => t.PhuongThuc == phuongThuc);
        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(t => t.TrangThai == trangThai);
        if (tuNgay.HasValue)
            query = query.Where(t => t.NgayThanhToan >= tuNgay.Value.Date);
        if (denNgay.HasValue)
            query = query.Where(t => t.NgayThanhToan < denNgay.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            if (int.TryParse(kw, out var ma))
                query = query.Where(t => t.MaHD == ma || t.MaTT == ma);
            else
                query = query.Where(t => t.MaGiaoDich != null && t.MaGiaoDich.Contains(kw));
        }

        ViewBag.PhuongThuc = phuongThuc;
        ViewBag.TrangThai = trangThai;
        ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
        ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");
        ViewBag.Q = q;
        ViewBag.PhuongThucs = PaymentService.PhuongThucThanhToan;

        var vm = new ThanhToanAdminIndexViewModel
        {
            TongGiaoDich = await query.CountAsync(),
            TongSoTien = await query.SumAsync(t => (decimal?)t.SoTien) ?? 0,
            TheoPhuongThuc = await query
                .GroupBy(t => t.PhuongThuc)
                .Select(g => new NhanLabelRow { Nhan = g.Key, GiaTri = g.Sum(x => x.SoTien) })
                .OrderByDescending(x => x.GiaTri)
                .ToListAsync(),
            DanhSach = await query
                .OrderByDescending(t => t.NgayThanhToan)
                .Take(200)
                .ToListAsync()
        };

        return View(vm);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var tt = await _db.ThanhToans
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.Ban)
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.KhachHang)
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.NhanVien)
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.KhuyenMai)
            .Include(t => t.HoaDon)
                .ThenInclude(h => h!.ChiTietHoaDons)
                .ThenInclude(c => c.SanPham)
            .FirstOrDefaultAsync(t => t.MaTT == id);

        if (tt == null) return NotFound();
        return View(tt);
    }
}