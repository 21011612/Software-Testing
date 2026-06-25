namespace QuanLyQuanCafe.Models.Atm;

public class BienLaiAtm
{
    public string SoThe { get; set; } = "";
    public string LoaiGiaoDich { get; set; } = "";
    public decimal? SoTienRut { get; set; }
    public decimal SoDuSauGiaoDich { get; set; }
    public DateTime ThoiGian { get; set; } = DateTime.Now;
}