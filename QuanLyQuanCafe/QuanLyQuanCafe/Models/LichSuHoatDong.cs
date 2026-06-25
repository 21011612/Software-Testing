namespace QuanLyQuanCafe.Models;

public class LichSuHoatDong
{
    public int MaLS { get; set; }
    public string LoaiHanhDong { get; set; } = "";
    public int? MaThamChieu { get; set; }
    public string? BangThamChieu { get; set; }
    public string? MoTa { get; set; }
    public int? MaNV { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? IP { get; set; }
    public NhanVien? NhanVien { get; set; }
}