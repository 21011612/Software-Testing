namespace QuanLyQuanCafe.Models;

public class KhuyenMai
{
    public int MaKM { get; set; }
    public string TenKM { get; set; } = "";
    public string? MoTa { get; set; }
    public string LoaiGiam { get; set; } = "PhanTram";
    public decimal GiaTriGiam { get; set; }
    public decimal DieuKienToiThieu { get; set; }
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public int SoLuongToiDa { get; set; }
    public int DaSuDung { get; set; }
    public bool TrangThai { get; set; } = true;
}