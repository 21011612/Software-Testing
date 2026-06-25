namespace QuanLyQuanCafe.Models;

public class KhachHang
{
    public int MaKH { get; set; }
    public string HoTen { get; set; } = "";
    public string SDT { get; set; } = "";
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public DateTime? NgaySinh { get; set; }
    public int DiemTichLuy { get; set; }
    public string HangThanhVien { get; set; } = "Thường";
    public DateTime NgayTao { get; set; }
    public string? GhiChu { get; set; }
    public TaiKhoan? TaiKhoan { get; set; }
}