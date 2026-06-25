using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafe.ViewModels.Atm;

public class NhapTheViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập số thẻ")]
    [StringLength(16, MinimumLength = 10, ErrorMessage = "Số thẻ từ 10–16 ký tự")]
    [Display(Name = "Số thẻ")]
    public string SoThe { get; set; } = "";
}

public class NhapPinViewModel
{
    public string SoThe { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập mã PIN")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN từ 4–6 số")]
    [RegularExpression(@"^\d+$", ErrorMessage = "PIN chỉ gồm chữ số")]
    [DataType(DataType.Password)]
    [Display(Name = "Mã PIN")]
    public string MaPin { get; set; } = "";
}

public class MenuAtmViewModel
{
    public string SoThe { get; set; } = "";
    public string ChuThe { get; set; } = "";
}

public class RutTienViewModel
{
    public string SoThe { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số tiền")]
    [Range(10000, 10000000, ErrorMessage = "Số tiền từ 10.000 đ đến 10.000.000 đ")]
    [Display(Name = "Số tiền muốn rút")]
    public decimal SoTien { get; set; }
}

public class KetQuaAtmViewModel
{
    public string TieuDe { get; set; } = "";
    public string ThongBao { get; set; } = "";
    public decimal? SoDu { get; set; }
    public decimal? SoTienRut { get; set; }
    public BienLaiAtmVm? BienLai { get; set; }
    public bool HienNutTiepTuc { get; set; }
}

public class BienLaiAtmVm
{
    public string SoThe { get; set; } = "";
    public string LoaiGiaoDich { get; set; } = "";
    public decimal? SoTienRut { get; set; }
    public decimal SoDuSauGiaoDich { get; set; }
    public DateTime ThoiGian { get; set; }
}

public static class LuaChonAtm
{
    public const string KiemTraSoDu = "balance";
    public const string RutTien = "withdraw";
    public const string Huy = "cancel";
}