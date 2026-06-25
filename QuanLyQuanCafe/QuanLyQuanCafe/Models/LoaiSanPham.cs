namespace QuanLyQuanCafe.Models;

public class LoaiSanPham
{
    public int MaLoai { get; set; }
    public string TenLoai { get; set; } = "";
    public string? MoTa { get; set; }
    public string? HinhAnh { get; set; }
    public int ThuTuHienThi { get; set; }
    public bool TrangThai { get; set; } = true;
    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}