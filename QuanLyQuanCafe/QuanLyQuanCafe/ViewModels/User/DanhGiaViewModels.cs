using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafe.ViewModels.User;

public class DanhGiaFormViewModel
{
    public int MaSP { get; set; }

    [Range(1, 5, ErrorMessage = "Chọn từ 1 đến 5 sao")]
    [Display(Name = "Số sao")]
    public byte SoSao { get; set; } = 5;

    [Required(ErrorMessage = "Vui lòng nhập nội dung")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Bình luận từ 5–500 ký tự")]
    [Display(Name = "Bình luận")]
    public string NoiDung { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Họ tên hiển thị")]
    public string? HoTenHienThi { get; set; }
}

public class DanhGiaHienThiViewModel
{
    public int MaDG { get; set; }
    public string HoTenHienThi { get; set; } = "";
    public byte SoSao { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayTao { get; set; }
    public bool LaCuaToi { get; set; }
}

public class StarRatingViewModel
{
    public double Diem { get; set; }
    public int SoLuot { get; set; }
    public string SizeClass { get; set; } = "";
}

public class SanPhamDanhGiaTongHop
{
    public int MaSP { get; set; }
    public double DiemTrungBinh { get; set; }
    public int SoDanhGia { get; set; }
}

public class QuanTriDanhGiaRowViewModel
{
    public int MaDG { get; set; }
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string HoTenHienThi { get; set; } = "";
    public byte SoSao { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayTao { get; set; }
    public bool TrangThai { get; set; }
}