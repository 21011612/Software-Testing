using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Controllers.User;

public class DanhGiaController : Controller
{
    private readonly DanhGiaService _danhGia;
    private readonly AuthService _auth;

    public DanhGiaController(DanhGiaService danhGia, AuthService auth)
    {
        _danhGia = danhGia;
        _auth = auth;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Gui(DanhGiaFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ReviewError"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Dữ liệu không hợp lệ.";
            return RedirectToAction("ChiTiet", "ThucDon", new { id = model.MaSP });
        }

        var maKh = _auth.GetMaKH();
        var hoTen = User.FindFirst("HoTen")?.Value;

        var (ok, err) = await _danhGia.LuuDanhGiaAsync(model, maKh, hoTen);
        if (!ok)
        {
            TempData["ReviewError"] = err;
            return RedirectToAction("ChiTiet", "ThucDon", new { id = model.MaSP });
        }

        TempData["Success"] = maKh.HasValue
            ? "Đã lưu đánh giá của bạn. Cảm ơn bạn!"
            : "Đã gửi bình luận. Cảm ơn bạn!";
        return RedirectToAction("ChiTiet", "ThucDon", new { id = model.MaSP });
    }
}