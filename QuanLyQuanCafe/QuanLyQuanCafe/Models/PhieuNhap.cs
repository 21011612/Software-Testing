namespace QuanLyQuanCafe.Models;

public class PhieuNhap
{
    public int MaPN { get; set; }
    public int MaNCC { get; set; }
    public int MaNV { get; set; }
    public DateTime NgayNhap { get; set; }
    public decimal TongTien { get; set; }
    public string? GhiChu { get; set; }
    public NhaCungCap? NhaCungCap { get; set; }
    public NhanVien? NhanVien { get; set; }
    public ICollection<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; } = new List<ChiTietPhieuNhap>();
}