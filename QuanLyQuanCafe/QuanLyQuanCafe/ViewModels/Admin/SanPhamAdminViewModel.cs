using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QuanLyQuanCafe.ViewModels.Admin;

public class SanPhamAdminViewModel
{
    public int MaSP { get; set; }

    [Required(ErrorMessage = "Nhập tên sản phẩm")]
    [Display(Name = "Tên sản phẩm")]
    public string TenSP { get; set; } = "";

    [Required]
    [Display(Name = "Loại")]
    public int MaLoai { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Display(Name = "Giá (đ)")]
    public decimal Gia { get; set; }

    [Display(Name = "Mô tả")]
    public string? MoTa { get; set; }

    [Display(Name = "Đường dẫn ảnh (tùy chọn nếu đã tải lên)")]
    public string? HinhAnh { get; set; }

    [Display(Name = "Tải ảnh từ máy")]
    public IFormFile? AnhUpload { get; set; }

    [Display(Name = "Đơn vị")]
    public string DonViTinh { get; set; } = "Ly";

    [Display(Name = "Kích cỡ")]
    public string KichCo { get; set; } = "M (Vừa)";

    [Display(Name = "Đang bán")]
    public bool TrangThai { get; set; } = true;
}