namespace QuanLyQuanCafe.Models;

public class NhaCungCap
{
    public int MaNCC { get; set; }
    public string TenNCC { get; set; } = "";
    public string? SDT { get; set; }
    public string? Email { get; set; }
    public string? DiaChi { get; set; }
    public string? NguoiLienHe { get; set; }
    public bool TrangThai { get; set; } = true;
    public ICollection<NguyenLieu> NguyenLieus { get; set; } = new List<NguyenLieu>();
}