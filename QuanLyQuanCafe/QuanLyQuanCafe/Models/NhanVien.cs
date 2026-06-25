namespace QuanLyQuanCafe.Models;

public class NhanVien
{
    public int MaNV { get; set; }
    public string HoTen { get; set; } = "";
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string SDT { get; set; } = "";
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string ChucVu { get; set; } = "";
    public DateTime NgayVaoLam { get; set; }
    public decimal LuongCoBan { get; set; }
    public bool TrangThai { get; set; } = true;
}