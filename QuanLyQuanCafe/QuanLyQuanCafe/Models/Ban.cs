namespace QuanLyQuanCafe.Models;

public class Ban
{
    public int MaBan { get; set; }
    public string TenBan { get; set; } = "";
    public string KhuVuc { get; set; } = "Tầng trệt";
    public int SoCho { get; set; } = 4;
    public string TrangThai { get; set; } = "Trống";
    public string? GhiChu { get; set; }
}