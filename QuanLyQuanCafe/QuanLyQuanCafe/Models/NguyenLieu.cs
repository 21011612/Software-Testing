namespace QuanLyQuanCafe.Models;

public class NguyenLieu
{
    public int MaNL { get; set; }
    public string TenNL { get; set; } = "";
    public string DonViTinh { get; set; } = "kg";
    public decimal SoLuongTon { get; set; }
    public decimal? GiaNhapTrungBinh { get; set; }
    public DateTime NgayCapNhat { get; set; }
    public int? MaNCC { get; set; }
    public NhaCungCap? NhaCungCap { get; set; }
}