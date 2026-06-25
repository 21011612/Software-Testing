using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriHoaDonController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly AdminOrderService _orders;
    private readonly PaymentService _payment;
    private readonly AuthService _auth;

    public QuanTriHoaDonController(
        TooruCoffeeDbContext db,
        AdminOrderService orders,
        PaymentService payment,
        AuthService auth)
    {
        _db = db;
        _orders = orders;
        _payment = payment;
        _auth = auth;
    }

    public async Task<IActionResult> Index(string? trangThai, string? loaiDon, DateTime? tuNgay, DateTime? denNgay)
    {
        var q = _db.HoaDons
            .Include(h => h.NhanVien)
            .Include(h => h.Ban)
            .Include(h => h.KhachHang)
            .AsQueryable();

        if (!string.IsNullOrEmpty(trangThai))
            q = q.Where(h => h.TrangThai == trangThai);
        if (!string.IsNullOrEmpty(loaiDon))
            q = q.Where(h => h.LoaiDon == loaiDon);
        if (tuNgay.HasValue)
            q = q.Where(h => h.NgayLap >= tuNgay.Value.Date);
        if (denNgay.HasValue)
            q = q.Where(h => h.NgayLap < denNgay.Value.Date.AddDays(1));

        ViewBag.TrangThai = trangThai;
        ViewBag.LoaiDon = loaiDon;
        ViewBag.TuNgay = tuNgay?.ToString("yyyy-MM-dd");
        ViewBag.DenNgay = denNgay?.ToString("yyyy-MM-dd");

        var list = await q.OrderByDescending(h => h.NgayLap).Take(100).ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Tao()
    {
        await LoadTaoDropdownsAsync();
        return View(new HoaDonTaoAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Tao(HoaDonTaoAdminViewModel model)
    {
        var maNv = _auth.GetMaNV();
        if (!maNv.HasValue)
        {
            ModelState.AddModelError("", "Tài khoản chưa gắn nhân viên (MaNV).");
            await LoadTaoDropdownsAsync();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            await LoadTaoDropdownsAsync();
            return View(model);
        }

        var (ok, maHd, err) = await _orders.TaoHoaDonAsync(model, maNv.Value);
        if (!ok)
        {
            ModelState.AddModelError("", err ?? "Không tạo được hóa đơn.");
            await LoadTaoDropdownsAsync();
            return View(model);
        }

        TempData["Success"] = "Đã tạo hóa đơn.";
        return RedirectToAction(nameof(ChiTiet), new { id = maHd });
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var hd = await _orders.LayChiTietAsync(id);
        if (hd == null) return NotFound();

        ViewBag.SanPhams = new SelectList(
            await _db.SanPhams.Where(s => s.TrangThai).OrderBy(s => s.TenSP).ToListAsync(),
            "MaSP", "TenSP");
        ViewBag.KhuyenMais = await _db.KhuyenMais
            .Where(k => k.TrangThai && k.NgayKetThuc >= DateTime.Today && k.DaSuDung < k.SoLuongToiDa)
            .OrderBy(k => k.TenKM)
            .ToListAsync();
        ViewBag.PhuongThucs = PaymentService.PhuongThucThanhToan;
        ViewBag.Line = new HoaDonChiTietLineViewModel();
        ViewBag.ThanhToan = new HoaDonThanhToanAdminViewModel { MaHD = id };
        ViewBag.MaKm = hd.MaKM;

        return View(hd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThemDong(int id, HoaDonChiTietLineViewModel line)
    {
        var (ok, err) = await _orders.ThemChiTietAsync(id, line);
        TempData[ok ? "Success" : "Error"] = ok ? "Đã thêm món." : err;
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XoaDong(int id, int maCt)
    {
        var (ok, err) = await _orders.XoaChiTietAsync(id, maCt);
        TempData[ok ? "Success" : "Error"] = ok ? "Đã xóa dòng." : err;
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApDungKm(int id, int maKm)
    {
        var (ok, err) = await _orders.ApDungKhuyenMaiAsync(id, maKm);
        TempData[ok ? "Success" : "Error"] = ok ? "Đã áp dụng khuyến mãi." : err;
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ThanhToan(int id, HoaDonThanhToanAdminViewModel model)
    {
        var maHd = id > 0 ? id : model.MaHD;
        if (maHd <= 0)
        {
            TempData["Error"] = "Không xác định được mã hóa đơn.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(model.PhuongThuc))
        {
            TempData["Error"] = "Chọn phương thức thanh toán.";
            return RedirectToAction(nameof(ChiTiet), new { id = maHd });
        }

        var hd = await _orders.LayChiTietAsync(maHd);
        if (hd == null) return NotFound();
        if (hd.TrangThai != "Chờ thanh toán")
        {
            TempData["Error"] = "Hóa đơn không ở trạng thái chờ thanh toán.";
            return RedirectToAction(nameof(ChiTiet), new { id = maHd });
        }

        if (hd.ChiTietHoaDons.Count == 0)
        {
            TempData["Error"] = "Hóa đơn chưa có món.";
            return RedirectToAction(nameof(ChiTiet), new { id = maHd });
        }

        var ttVm = new ThanhToanViewModel
        {
            MaHD = maHd,
            PhuongThuc = model.PhuongThuc.Trim(),
            MaGiaoDich = model.MaGiaoDich,
            TongThanhToan = hd.TongThanhToan
        };

        var (ok, err) = await _payment.ThanhToanAsync(ttVm, _auth.GetMaNV());
        TempData[ok ? "Success" : "Error"] = ok ? "Thanh toán thành công." : err;
        return RedirectToAction(nameof(ChiTiet), new { id = maHd });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Huy(int id)
    {
        var (ok, err) = await _orders.HuyHoaDonAsync(id, _auth.GetMaNV());
        TempData[ok ? "Success" : "Error"] = ok ? "Đã hủy hóa đơn." : err;
        return RedirectToAction(nameof(ChiTiet), new { id });
    }

    private async Task LoadTaoDropdownsAsync()
    {
        ViewBag.Bans = new SelectList(
            await _db.Bans.OrderBy(b => b.KhuVuc).ThenBy(b => b.TenBan).ToListAsync(),
            "MaBan", "TenBan");
        ViewBag.KhachHangs = new SelectList(
            await _db.KhachHangs.OrderBy(k => k.HoTen).ToListAsync(),
            "MaKH", "HoTen");
    }
}