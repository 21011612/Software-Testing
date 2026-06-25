namespace QuanLyQuanCafe.Models.Atm;

/// <summary>Dữ liệu tài khoản trên Server Ngân Hàng (database).</summary>
public class TaiKhoanNganHang
{
    public string SoThe { get; set; } = "";
    public string MaPin { get; set; } = "";
    public string ChuThe { get; set; } = "";
    public decimal SoDu { get; set; }
}