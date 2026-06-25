using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafe.ViewModels.User;

public class DatHangViewModel
{
    [Display(Name = "Loại đơn")]
    public string LoaiDon { get; set; } = "Tại quán";

    [Display(Name = "Bàn")]
    public int? MaBan { get; set; }

    [Display(Name = "Mã khuyến mãi")]
    public int? MaKM { get; set; }

    [Display(Name = "Tên khách")]
    public string? TenKhach { get; set; }

    [Display(Name = "Số điện thoại")]
    public string? SDT { get; set; }

    [Display(Name = "Địa chỉ giao")]
    public string? DiaChiGiao { get; set; }

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}