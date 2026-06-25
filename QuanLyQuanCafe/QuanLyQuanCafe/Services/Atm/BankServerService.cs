using QuanLyQuanCafe.Models.Atm;

namespace QuanLyQuanCafe.Services.Atm;

/// <summary>Server Ngân Hàng — lớp truy cập dữ liệu (Model persistence).</summary>
public class BankServerService
{
    private readonly Dictionary<string, TaiKhoanNganHang> _taiKhoan = new(StringComparer.Ordinal)
    {
        ["1234567890"] = new() { SoThe = "1234567890", MaPin = "1234", ChuThe = "Nguyen Van A", SoDu = 5_000_000 },
        ["0987654321"] = new() { SoThe = "0987654321", MaPin = "0000", ChuThe = "Tran Thi B", SoDu = 500_000 },
    };

    public bool TheTonTai(string soThe) => _taiKhoan.ContainsKey(soThe.Trim());

    public bool KiemTraMaPin(string soThe, string maPin)
    {
        if (!_taiKhoan.TryGetValue(soThe.Trim(), out var tk))
            return false;
        return tk.MaPin == maPin;
    }

    public TaiKhoanNganHang? LayTaiKhoan(string soThe)
    {
        _taiKhoan.TryGetValue(soThe.Trim(), out var tk);
        return tk;
    }

    public decimal? LaySoDu(string soThe) => LayTaiKhoan(soThe)?.SoDu;

    public bool CapNhatSoDu(string soThe, decimal soTienRut, out decimal soDuMoi)
    {
        soDuMoi = 0;
        if (!_taiKhoan.TryGetValue(soThe.Trim(), out var tk))
            return false;
        if (tk.SoDu < soTienRut)
            return false;
        tk.SoDu -= soTienRut;
        soDuMoi = tk.SoDu;
        return true;
    }
}