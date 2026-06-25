using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafe.ViewModels.Admin;

public class BanAdminViewModel
{
    public int MaBan { get; set; }

    [Required]
    [Display(Name = "Tên bàn")]
    public string TenBan { get; set; } = "";

    [Display(Name = "Khu vực")]
    public string KhuVuc { get; set; } = "Tầng trệt";

    [Range(1, 20)]
    [Display(Name = "Số chỗ")]
    public int SoCho { get; set; } = 4;

    [Required]
    [Display(Name = "Trạng thái")]
    public string TrangThai { get; set; } = "Trống";

    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}