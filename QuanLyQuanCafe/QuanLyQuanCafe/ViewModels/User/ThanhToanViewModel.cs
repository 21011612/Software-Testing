using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafe.ViewModels.User;

public class ThanhToanViewModel
{
    public int MaHD { get; set; }
    public decimal TongThanhToan { get; set; }
    public decimal TongTienTruocGiam { get; set; }
    public decimal GiamGia { get; set; }
    public string TrangThai { get; set; } = "";
    public List<GioHangItem> ChiTiet { get; set; } = new();

    [Required(ErrorMessage = "Chọn phương thức thanh toán")]
    [Display(Name = "Phương thức thanh toán")]
    public string PhuongThuc { get; set; } = "Tiền mặt";

    [Display(Name = "Mã giao dịch (nếu có)")]
    public string? MaGiaoDich { get; set; }
}