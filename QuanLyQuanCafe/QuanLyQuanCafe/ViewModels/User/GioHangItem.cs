namespace QuanLyQuanCafe.ViewModels.User;

public class GioHangItem
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string HinhAnh { get; set; } = "";
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; } = 1;
    public string? GhiChu { get; set; }
    public decimal ThanhTien => DonGia * SoLuong;
}