namespace QuanLyQuanCafe.Models;

public class SanPham
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public int MaLoai { get; set; }
    public decimal Gia { get; set; }
    public string? MoTa { get; set; }
    public string HinhAnh { get; set; } = "";
    public string DonViTinh { get; set; } = "Ly";
    public string KichCo { get; set; } = "M (Vừa)";
    public bool TrangThai { get; set; } = true;
    public DateTime NgayTao { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public LoaiSanPham? LoaiSanPham { get; set; }
}