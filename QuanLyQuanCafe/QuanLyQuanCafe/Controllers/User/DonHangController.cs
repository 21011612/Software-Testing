using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Controllers.User;

public class DonHangController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly CartService _cart;
    private readonly OrderService _orders;
    private readonly AuthService _auth;

    public DonHangController(TooruCoffeeDbContext db, CartService cart, OrderService orders, AuthService auth)
    {
        _db = db;
        _cart = cart;
        _orders = orders;
        _auth = auth;
    }

    public IActionResult GioHang()
    {
        ViewBag.Items = _cart.GetItems();
        ViewBag.TongTien = _cart.TongTien();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CapNhatGioHang(int maSp, int soLuong)
    {
        _cart.Update(maSp, soLuong);
        return RedirectToAction(nameof(GioHang));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TangGiamGioHang(int maSp, int delta)
    {
        _cart.ChangeQuantity(maSp, delta);
        return RedirectToAction(nameof(GioHang));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult XoaGioHang(int maSp)
    {
        _cart.Remove(maSp);
        return RedirectToAction(nameof(GioHang));
    }

    [Authorize]
    public async Task<IActionResult> DatHang()
    {
        if (_cart.GetItems().Count == 0)
        {
            TempData["Error"] = "Giỏ hàng trống.";
            return RedirectToAction("Index", "ThucDon");
        }

        var kh = _auth.GetMaKH();
        KhachHang? khach = null;
        if (kh.HasValue)
            khach = await _db.KhachHangs.FindAsync(kh.Value);

        ViewBag.Items = _cart.GetItems();
        ViewBag.TongTien = _cart.TongTien();
        ViewBag.Bans = await _db.Bans.Where(b => b.TrangThai == "Trống" || b.TrangThai == "Đang phục vụ").ToListAsync();
        ViewBag.KhuyenMais = await _db.KhuyenMais
            .Where(k => k.TrangThai && k.NgayBatDau <= DateTime.Today && k.NgayKetThuc >= DateTime.Today)
            .ToListAsync();

        return View(new DatHangViewModel
        {
            LoaiDon = "Tại quán",
            TenKhach = khach?.HoTen,
            SDT = khach?.SDT
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DatHang(DatHangViewModel model)
    {
        var items = _cart.GetItems();
        if (items.Count == 0)
        {
            TempData["Error"] = "Giỏ hàng trống.";
            return RedirectToAction("Index", "ThucDon");
        }

        var maNv = _auth.GetMaNV();
        if (!maNv.HasValue)
        {
            var nvMacDinh = await _db.NhanViens.Where(n => n.TrangThai).Select(n => (int?)n.MaNV).FirstOrDefaultAsync();
            maNv = nvMacDinh;
        }
        if (!maNv.HasValue)
        {
            TempData["Error"] = "Không tìm thấy nhân viên xử lý đơn.";
            return RedirectToAction(nameof(GioHang));
        }

        try
        {
            var maHd = await _orders.TaoHoaDonAsync(model, maNv.Value, _auth.GetMaKH(), items);
            _cart.Clear();
            TempData["Success"] = $"Đặt hàng thành công! Mã hóa đơn: #{maHd}";
            return RedirectToAction("ChiTiet", "ThanhToan", new { id = maHd });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(GioHang));
        }
    }

    [Authorize(Roles = "KhachHang")]
    public async Task<IActionResult> LichSu()
    {
        var maKh = _auth.GetMaKH();
        if (!maKh.HasValue)
            return RedirectToAction("DangNhap", "TaiKhoan");

        var hoaDons = await _db.HoaDons
            .Where(h => h.MaKH == maKh)
            .OrderByDescending(h => h.NgayLap)
            .Take(20)
            .ToListAsync();
        return View(hoaDons);
    }
}