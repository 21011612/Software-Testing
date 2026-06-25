namespace QuanLyQuanCafe.Models;

public class ChiTietHoaDon
{
    public int MaCT { get; set; }
    public int MaHD { get; set; }
    public int MaSP { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string? GhiChu { get; set; }
    public decimal ThanhTien { get; set; }
    public HoaDon? HoaDon { get; set; }
    public SanPham? SanPham { get; set; }
}