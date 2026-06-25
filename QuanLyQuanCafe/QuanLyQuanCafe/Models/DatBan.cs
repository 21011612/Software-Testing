namespace QuanLyQuanCafe.Models;

public class DatBan
{
    public int MaDatBan { get; set; }
    public int MaBan { get; set; }
    public int? MaKH { get; set; }
    public string TenKhach { get; set; } = "";
    public string SDT { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public TimeSpan GioBatDau { get; set; }
    public TimeSpan? GioKetThuc { get; set; }
    public int SoNguoi { get; set; }
    public string TrangThai { get; set; } = "Đã xác nhận";
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public Ban? Ban { get; set; }
    public KhachHang? KhachHang { get; set; }
}