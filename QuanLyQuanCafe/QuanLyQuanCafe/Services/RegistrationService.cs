using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services;

public class RegistrationService
{
    private readonly TooruCoffeeDbContext _db;

    public RegistrationService(TooruCoffeeDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Success, string? Error)> DangKyAsync(DangKyViewModel model)
    {
        if (await _db.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
            return (false, "Tên đăng nhập đã tồn tại.");
        if (await _db.KhachHangs.AnyAsync(k => k.SDT == model.SDT))
            return (false, "Số điện thoại đã được đăng ký.");

        var kh = new KhachHang
        {
            HoTen = model.HoTen,
            SDT = model.SDT,
            Email = model.Email,
            DiaChi = model.DiaChi,
            DiemTichLuy = 0,
            HangThanhVien = "Thường",
            NgayTao = DateTime.Now
        };
        _db.KhachHangs.Add(kh);
        await _db.SaveChangesAsync();

        var tk = new TaiKhoan
        {
            TenDangNhap = model.TenDangNhap,
            MatKhauHash = PasswordHelper.Hash(model.MatKhau),
            VaiTro = "KhachHang",
            MaKH = kh.MaKH,
            TrangThai = true,
            NgayTao = DateTime.Now
        };
        _db.TaiKhoans.Add(tk);
        await _db.SaveChangesAsync();

        return (true, null);
    }
}
