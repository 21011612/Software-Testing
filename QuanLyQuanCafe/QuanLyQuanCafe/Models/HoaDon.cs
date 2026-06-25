namespace QuanLyQuanCafe.Models;

public class HoaDon
{
    public int MaHD { get; set; }
    public int? MaBan { get; set; }
    public int MaNV { get; set; }
    public int? MaKH { get; set; }
    public int? MaKM { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TongTienTruocGiam { get; set; }
    public decimal GiamGia { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThai { get; set; } = "Chờ thanh toán";
    public string LoaiDon { get; set; } = "Tại quán";
    public string? TenKhach { get; set; }
    public string? SDT { get; set; }
    public string? DiaChiGiao { get; set; }
    public string? GhiChu { get; set; }
    public Ban? Ban { get; set; }
    public NhanVien? NhanVien { get; set; }
    public KhachHang? KhachHang { get; set; }
    public KhuyenMai? KhuyenMai { get; set; }
    public ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();
    public ThanhToan? ThanhToan { get; set; }
}