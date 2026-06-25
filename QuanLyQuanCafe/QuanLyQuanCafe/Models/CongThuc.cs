namespace QuanLyQuanCafe.Models;

public class CongThuc
{
    public int MaCongThuc { get; set; }
    public int MaSP { get; set; }
    public int MaNL { get; set; }
    public decimal SoLuongCan { get; set; }
    public SanPham? SanPham { get; set; }
    public NguyenLieu? NguyenLieu { get; set; }
}