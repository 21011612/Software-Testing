namespace QuanLyQuanCafe.Models;

public class ChiTietPhieuNhap
{
    public int MaCTPN { get; set; }
    public int MaPN { get; set; }
    public int MaNL { get; set; }
    public decimal SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public PhieuNhap? PhieuNhap { get; set; }
    public NguyenLieu? NguyenLieu { get; set; }
}