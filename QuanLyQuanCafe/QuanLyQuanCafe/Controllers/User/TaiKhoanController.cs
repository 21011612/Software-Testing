using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Controllers.User;

public class TaiKhoanController : Controller
{
    private readonly AuthService _auth;
    private readonly RegistrationService _registration;

    public TaiKhoanController(AuthService auth, RegistrationService registration)
    {
        _auth = auth;
        _registration = registration;
    }

    [AllowAnonymous]
    public async Task<IActionResult> DangNhap(string? returnUrl = null, string? tenDangNhap = null, bool daDangKy = false)
    {
        // Vừa đăng ký xong: luôn hiện form đăng nhập, không tự vào Thực đơn
        if (daDangKy)
            await _auth.DangXuatAsync();
        else if (User.IsInRole("KhachHang"))
            return RedirectToAction("Index", "ThucDon");
        else if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
            return RedirectToAction("Index", "QuanTri");

        return View(new DangNhapViewModel
        {
            ReturnUrl = returnUrl,
            TenDangNhap = tenDangNhap ?? ""
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangNhap(DangNhapViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, err) = await _auth.DangNhapKhachHangAsync(model);
        if (!ok)
        {
            ModelState.AddModelError("", err ?? "Đăng nhập thất bại.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "ThucDon");
    }

    [AllowAnonymous]
    public IActionResult DangKy() => View(new DangKyViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DangKy(DangKyViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var (ok, err) = await _registration.DangKyAsync(model);
        if (!ok)
        {
            ModelState.AddModelError("", err ?? "Đăng ký thất bại.");
            return View(model);
        }

        await _auth.DangXuatAsync();

        TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập để tiếp tục.";
        return RedirectToAction(nameof(DangNhap), new { tenDangNhap = model.TenDangNhap, daDangKy = true });
    }

    [Authorize]
    public async Task<IActionResult> DangXuat()
    {
        await _auth.DangXuatAsync();
        return RedirectToAction("Index", "Home");
    }
}