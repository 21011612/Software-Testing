using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Controllers.User;

public class ThucDonController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly CartService _cart;
    private readonly DanhGiaService _danhGia;
    private readonly AuthService _auth;

    public ThucDonController(TooruCoffeeDbContext db, CartService cart, DanhGiaService danhGia, AuthService auth)
    {
        _db = db;
        _cart = cart;
        _danhGia = danhGia;
        _auth = auth;
    }

    public async Task<IActionResult> Index(int? loai, string? q, string? sort)
    {
        var loais = await _db.LoaiSanPhams
            .Where(l => l.TrangThai)
            .OrderBy(l => l.ThuTuHienThi)
            .ToListAsync();

        var query = _db.SanPhams
            .Include(s => s.LoaiSanPham)
            .Where(s => s.TrangThai && s.LoaiSanPham!.TrangThai);

        if (loai.HasValue)
            query = query.Where(s => s.MaLoai == loai.Value);

        var tuKhoa = q?.Trim();
        if (!string.IsNullOrEmpty(tuKhoa))
        {
            query = query.Where(s =>
                s.TenSP.Contains(tuKhoa) ||
                (s.MoTa != null && s.MoTa.Contains(tuKhoa)) ||
                (s.LoaiSanPham != null && s.LoaiSanPham.TenLoai.Contains(tuKhoa)));
        }

        query = sort switch
        {
            "gia-tang" => query.OrderBy(s => s.Gia),
            "gia-giam" => query.OrderByDescending(s => s.Gia),
            "ten" => query.OrderBy(s => s.TenSP),
            _ => query.OrderBy(s => s.MaLoai).ThenBy(s => s.TenSP)
        };

        var sanPhams = await query.ToListAsync();
        ViewBag.DanhGiaTongHop = await _danhGia.LayTongHopNhieuAsync(sanPhams.Select(s => s.MaSP));

        ViewBag.Loais = loais;
        ViewBag.LoaiChon = loai;
        ViewBag.TuKhoa = tuKhoa;
        ViewBag.Sort = sort ?? "mac-dinh";
        ViewBag.SoMonGioHang = _cart.SoMon();
        return View(sanPhams);
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var sp = await _db.SanPhams
            .Include(s => s.LoaiSanPham)
            .FirstOrDefaultAsync(s => s.MaSP == id && s.TrangThai);
        if (sp == null) return NotFound();

        var maKh = _auth.GetMaKH();
        ViewBag.ImageUrl = ImageHelper.ToUrl(sp.HinhAnh);
        ViewBag.SoMonGioHang = _cart.SoMon();
        ViewBag.DanhGiaTongHop = await _danhGia.LayTongHopAsync(sp.MaSP);
        ViewBag.DanhGiaList = await _danhGia.LayDanhSachHienThiAsync(sp.MaSP, maKh);
        ViewBag.DanhGiaCuaToi = maKh.HasValue ? await _danhGia.LayDanhGiaCuaKhachAsync(sp.MaSP, maKh.Value) : null;
        ViewBag.MaKhachHang = maKh;
        ViewBag.HoTenKhach = User.FindFirst("HoTen")?.Value;
        return View(sp);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ThemGioHang(int maSp, string tenSp, string hinhAnh, decimal donGia, int soLuong = 1, string? ghiChu = null)
    {
        if (soLuong < 1) soLuong = 1;
        _cart.Add(new GioHangItem
        {
            MaSP = maSp,
            TenSP = tenSp,
            HinhAnh = hinhAnh,
            DonGia = donGia,
            SoLuong = soLuong,
            GhiChu = ghiChu
        });
        TempData["Success"] = $"Đã thêm {tenSp} vào giỏ hàng.";
        return RedirectToAction(nameof(Index));
    }
}