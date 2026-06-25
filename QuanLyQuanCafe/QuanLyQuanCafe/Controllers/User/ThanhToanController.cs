using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Controllers.User;

[Authorize]
public class ThanhToanController : Controller
{
    private readonly OrderService _orders;
    private readonly PaymentService _payment;
    private readonly AuthService _auth;

    public ThanhToanController(OrderService orders, PaymentService payment, AuthService auth)
    {
        _orders = orders;
        _payment = payment;
        _auth = auth;
    }

    public async Task<IActionResult> ChiTiet(int id)
    {
        var vm = await _orders.GetHoaDonThanhToanAsync(id);
        if (vm == null) return NotFound();

        if (vm.TrangThai != "Chờ thanh toán")
        {
            ViewBag.DaThanhToan = true;
            return View("HoanTat", vm);
        }

        ViewBag.PhuongThucs = PaymentService.PhuongThucThanhToan;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> XacNhan(ThanhToanViewModel model)
    {
        if (model.MaHD <= 0)
        {
            TempData["Error"] = "Không xác định được hóa đơn.";
            return RedirectToAction("GioHang", "DonHang");
        }

        if (string.IsNullOrWhiteSpace(model.PhuongThuc))
        {
            TempData["Error"] = "Chọn phương thức thanh toán.";
            return RedirectToAction(nameof(ChiTiet), new { id = model.MaHD });
        }

        var hd = await _orders.GetHoaDonThanhToanAsync(model.MaHD);
        if (hd == null) return NotFound();
        if (hd.TrangThai != "Chờ thanh toán")
        {
            TempData["Error"] = "Hóa đơn đã được thanh toán hoặc đã hủy.";
            return RedirectToAction(nameof(ChiTiet), new { id = model.MaHD });
        }

        model.TongThanhToan = hd.TongThanhToan;
        var (ok, err) = await _payment.ThanhToanAsync(model, _auth.GetMaNV());
        if (!ok)
        {
            TempData["Error"] = err;
            return RedirectToAction(nameof(ChiTiet), new { id = model.MaHD });
        }

        TempData["Success"] = "Thanh toán thành công! Cảm ơn bạn đã ghé Tooru Coffee.";
        return RedirectToAction(nameof(ChiTiet), new { id = model.MaHD });
    }
}