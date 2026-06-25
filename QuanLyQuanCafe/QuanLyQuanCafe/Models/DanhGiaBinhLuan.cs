namespace QuanLyQuanCafe.Models;

public class DanhGiaBinhLuan
{
    public int MaDG { get; set; }
    public int MaSP { get; set; }
    public int? MaKH { get; set; }
    public string HoTenHienThi { get; set; } = "";
    public byte SoSao { get; set; }
    public string NoiDung { get; set; } = "";
    public DateTime NgayTao { get; set; }
    public bool TrangThai { get; set; } = true;

    public SanPham? SanPham { get; set; }
    public KhachHang? KhachHang { get; set; }
}