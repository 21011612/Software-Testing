namespace QuanLyQuanCafe.Models;

public class ThanhToan
{
    public int MaTT { get; set; }
    public int MaHD { get; set; }
    public DateTime NgayThanhToan { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThuc { get; set; } = "";
    public string? MaGiaoDich { get; set; }
    public string TrangThai { get; set; } = "Thành công";
    public HoaDon? HoaDon { get; set; }
}