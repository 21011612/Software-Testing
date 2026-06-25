using System.ComponentModel.DataAnnotations;
using QuanLyQuanCafe.Models;

namespace QuanLyQuanCafe.ViewModels.Admin;

public class KhachHangAdminViewModel
{
    public int MaKH { get; set; }

    [Required(ErrorMessage = "Nhập họ tên")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Nhập SĐT")]
    [Phone]
    [Display(Name = "Số điện thoại")]
    public string SDT { get; set; } = "";

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Điểm tích lũy")]
    public int DiemTichLuy { get; set; }

    [Required]
    [Display(Name = "Hạng thành viên")]
    public string HangThanhVien { get; set; } = "Thường";

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class KhuyenMaiAdminViewModel
{
    public int MaKM { get; set; }

    [Required]
    [Display(Name = "Tên khuyến mãi")]
    public string TenKM { get; set; } = "";

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Required]
    [Display(Name = "Loại giảm")]
    public string LoaiGiam { get; set; } = "PhanTram";

    [Range(0, double.MaxValue)]
    [Display(Name = "Giá trị giảm")]
    public decimal GiaTriGiam { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Đơn tối thiểu (đ)")]
    public decimal DieuKienToiThieu { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày bắt đầu")]
    public DateTime NgayBatDau { get; set; } = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày kết thúc")]
    public DateTime NgayKetThuc { get; set; } = DateTime.Today.AddMonths(1);

    [Range(1, 99999)]
    [Display(Name = "Số lượt tối đa")]
    public int SoLuongToiDa { get; set; } = 9999;

    [Display(Name = "Đang kích hoạt")]
    public bool TrangThai { get; set; } = true;
}

public class NhanVienAdminViewModel
{
    public int MaNV { get; set; }

    [Required]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Ngày sinh")]
    public DateTime? NgaySinh { get; set; }

    [Display(Name = "Giới tính")]
    public string? GioiTinh { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Số điện thoại")]
    public string SDT { get; set; } = "";

    [EmailAddress]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? DiaChi { get; set; }

    [Required]
    [Display(Name = "Chức vụ")]
    public string ChucVu { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Ngày vào làm")]
    public DateTime NgayVaoLam { get; set; } = DateTime.Today;

    [Range(0, double.MaxValue)]
    [Display(Name = "Lương cơ bản")]
    public decimal LuongCoBan { get; set; }

    [Display(Name = "Đang làm việc")]
    public bool TrangThai { get; set; } = true;
}

public class DatBanAdminViewModel
{
    public int MaDatBan { get; set; }

    [Required]
    [Display(Name = "Bàn")]
    public int MaBan { get; set; }

    [Display(Name = "Khách hàng (nếu có)")]
    public int? MaKH { get; set; }

    [Required]
    [Display(Name = "Tên khách")]
    public string TenKhach { get; set; } = "";

    [Required]
    [Phone]
    [Display(Name = "Số điện thoại")]
    public string SDT { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày đặt")]
    public DateTime NgayDat { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Giờ bắt đầu")]
    public string GioBatDau { get; set; } = "09:00";

    [Display(Name = "Giờ kết thúc")]
    public string? GioKetThuc { get; set; }

    [Range(1, 30)]
    [Display(Name = "Số người")]
    public int SoNguoi { get; set; } = 2;

    [Required]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "Đã xác nhận";

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public class LoaiSanPhamAdminViewModel
{
    public int MaLoai { get; set; }
    [Required][Display(Name = "Tên loại")] public string TenLoai { get; set; } = "";
    [Display(Name = "Mô tả")] public string? MoTa { get; set; }
    [Display(Name = "Hình ảnh")] public string? HinhAnh { get; set; }
    [Display(Name = "Thứ tự")] public int ThuTuHienThi { get; set; }
    [Display(Name = "Hiển thị")] public bool TrangThai { get; set; } = true;
}

public class TaiKhoanAdminViewModel
{
    public int MaTK { get; set; }
    [Required][Display(Name = "Tên đăng nhập")] public string TenDangNhap { get; set; } = "";
    [Display(Name = "Mật khẩu mới")][DataType(DataType.Password)] public string? MatKhauMoi { get; set; }
    [Required][Display(Name = "Vai trò")] public string VaiTro { get; set; } = "KhachHang";
    [Display(Name = "Nhân viên")] public int? MaNV { get; set; }
    [Display(Name = "Khách hàng")] public int? MaKH { get; set; }
    [Display(Name = "Kích hoạt")] public bool TrangThai { get; set; } = true;
}

public class NhaCungCapAdminViewModel
{
    public int MaNCC { get; set; }
    [Required][Display(Name = "Tên NCC")] public string TenNCC { get; set; } = "";
    [Phone][Display(Name = "SĐT")] public string? SDT { get; set; }
    [EmailAddress][Display(Name = "Email")] public string? Email { get; set; }
    [Display(Name = "Địa chỉ")] public string? DiaChi { get; set; }
    [Display(Name = "Liên hệ")] public string? NguoiLienHe { get; set; }
    [Display(Name = "Hoạt động")] public bool TrangThai { get; set; } = true;
}

public class NguyenLieuAdminViewModel
{
    public int MaNL { get; set; }
    [Required][Display(Name = "Tên NL")] public string TenNL { get; set; } = "";
    [Display(Name = "ĐVT")] public string DonViTinh { get; set; } = "kg";
    [Range(0, double.MaxValue)][Display(Name = "Tồn")] public decimal SoLuongTon { get; set; }
    [Display(Name = "Giá nhập")] public decimal? GiaNhapTrungBinh { get; set; }
    [Display(Name = "NCC")] public int? MaNCC { get; set; }
}

public class PhieuNhapAdminViewModel
{
    public int MaPN { get; set; }
    [Required][Display(Name = "NCC")] public int MaNCC { get; set; }
    [Required][Display(Name = "NV")] public int MaNV { get; set; }
    [Display(Name = "Ghi chú")] public string? GhiChu { get; set; }
}

public class ChiTietPhieuNhapLineViewModel
{
    [Required] public int MaNL { get; set; }
    [Range(0.001, double.MaxValue)] public decimal SoLuong { get; set; }
    [Range(0, double.MaxValue)] public decimal DonGia { get; set; }
}

public class CongThucAdminViewModel
{
    public int MaCongThuc { get; set; }
    [Required] public int MaSP { get; set; }
    [Required] public int MaNL { get; set; }
    [Range(0.001, double.MaxValue)] public decimal SoLuongCan { get; set; }
}

public class HoaDonTaoAdminViewModel
{
    [Required] public string LoaiDon { get; set; } = "Tại quán";
    public int? MaBan { get; set; }
    public int? MaKH { get; set; }
    public string? TenKhach { get; set; }
    public string? SDT { get; set; }
    public string? DiaChiGiao { get; set; }
    public string? GhiChu { get; set; }
}

public class HoaDonChiTietLineViewModel
{
    [Required] public int MaSP { get; set; }
    [Range(1, 99)] public int SoLuong { get; set; } = 1;
    public string? GhiChu { get; set; }
}

public class HoaDonThanhToanAdminViewModel
{
    public int MaHD { get; set; }
    [Required] public string PhuongThuc { get; set; } = "Tiền mặt";
    public string? MaGiaoDich { get; set; }
}

public class ThanhToanAdminIndexViewModel
{
    public List<ThanhToan> DanhSach { get; set; } = [];
    public int TongGiaoDich { get; set; }
    public decimal TongSoTien { get; set; }
    public List<NhanLabelRow> TheoPhuongThuc { get; set; } = [];
}