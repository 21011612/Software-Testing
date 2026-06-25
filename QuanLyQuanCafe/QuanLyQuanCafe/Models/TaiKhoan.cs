namespace QuanLyQuanCafe.Models;

public class TaiKhoan
{
    public int MaTK { get; set; }
    public string TenDangNhap { get; set; } = "";
    public string MatKhauHash { get; set; } = "";
    public string VaiTro { get; set; } = "";
    public int? MaNV { get; set; }
    public int? MaKH { get; set; }
    public bool TrangThai { get; set; } = true;
    public DateTime? LanDangNhapCuoi { get; set; }
    public DateTime NgayTao { get; set; }
    public NhanVien? NhanVien { get; set; }
    public KhachHang? KhachHang { get; set; }
}