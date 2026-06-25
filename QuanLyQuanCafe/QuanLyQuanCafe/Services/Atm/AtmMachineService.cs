using QuanLyQuanCafe.Models.Atm;
using QuanLyQuanCafe.ViewModels.Atm;

namespace QuanLyQuanCafe.Services.Atm;

/// <summary>Cây ATM — điều phối luồng theo sequence diagram (gọi BankServer).</summary>
public class AtmMachineService
{
    private readonly BankServerService _bank;
    private readonly IHttpContextAccessor _http;

    public AtmMachineService(BankServerService bank, IHttpContextAccessor http)
    {
        _bank = bank;
        _http = http;
    }

    private ISession Session => _http.HttpContext!.Session;

    public void XoaPhien()
    {
        Session.Remove(AtmSessionKeys.SoThe);
        Session.Remove(AtmSessionKeys.ChuThe);
        Session.Remove(AtmSessionKeys.DaXacThucPin);
        Session.Remove(AtmSessionKeys.BienLai);
    }

    public (bool Ok, string? Error) NhapThe(string soThe)
    {
        soThe = soThe.Trim();
        if (!_bank.TheTonTai(soThe))
            return (false, "Thẻ không hợp lệ hoặc không tồn tại trên hệ thống.");
        var tk = _bank.LayTaiKhoan(soThe)!;
        Session.SetString(AtmSessionKeys.SoThe, soThe);
        Session.SetString(AtmSessionKeys.ChuThe, tk.ChuThe);
        Session.SetString(AtmSessionKeys.DaXacThucPin, "0");
        return (true, null);
    }

    public (bool Ok, string? Error) XacNhanPin(string maPin)
    {
        var soThe = Session.GetString(AtmSessionKeys.SoThe);
        if (string.IsNullOrEmpty(soThe))
            return (false, "Phiên ATM đã hết. Vui lòng nhập thẻ lại.");
        if (!_bank.KiemTraMaPin(soThe, maPin))
            return (false, "Mã PIN không đúng.");
        Session.SetString(AtmSessionKeys.DaXacThucPin, "1");
        return (true, null);
    }

    public bool DaDangNhap()
    {
        return Session.GetString(AtmSessionKeys.DaXacThucPin) == "1"
            && !string.IsNullOrEmpty(Session.GetString(AtmSessionKeys.SoThe));
    }

    public MenuAtmViewModel? LayMenu()
    {
        if (!DaDangNhap()) return null;
        return new MenuAtmViewModel
        {
            SoThe = Session.GetString(AtmSessionKeys.SoThe) ?? "",
            ChuThe = Session.GetString(AtmSessionKeys.ChuThe) ?? "",
        };
    }

    public KetQuaAtmViewModel KiemTraSoDu()
    {
        var soThe = Session.GetString(AtmSessionKeys.SoThe)!;
        var soDu = _bank.LaySoDu(soThe) ?? 0;
        var bienLai = TaoBienLai(soThe, "Kiểm tra số dư", null, soDu);
        LuuBienLai(bienLai);
        return new KetQuaAtmViewModel
        {
            TieuDe = "Số dư tài khoản",
            ThongBao = $"Số dư hiện tại: {soDu:N0} đ",
            SoDu = soDu,
            BienLai = MapBienLai(bienLai),
            HienNutTiepTuc = true,
        };
    }

    public KetQuaAtmViewModel RutTien(decimal soTien)
    {
        var soThe = Session.GetString(AtmSessionKeys.SoThe)!;
        var soDu = _bank.LaySoDu(soThe) ?? 0;

        if (soDu < soTien)
        {
            return new KetQuaAtmViewModel
            {
                TieuDe = "Rút tiền thất bại",
                ThongBao = "Số dư không đủ để giao dịch",
                SoDu = soDu,
                HienNutTiepTuc = true,
            };
        }

        if (!_bank.CapNhatSoDu(soThe, soTien, out var soDuMoi))
        {
            return new KetQuaAtmViewModel
            {
                TieuDe = "Rút tiền thất bại",
                ThongBao = "Không thể cập nhật số dư. Vui lòng thử lại.",
                HienNutTiepTuc = true,
            };
        }

        var bienLai = TaoBienLai(soThe, "Rút tiền", soTien, soDuMoi);
        LuuBienLai(bienLai);
        return new KetQuaAtmViewModel
        {
            TieuDe = "Rút tiền thành công",
            ThongBao = "Hãy nhận tiền",
            SoTienRut = soTien,
            SoDu = soDuMoi,
            BienLai = MapBienLai(bienLai),
            HienNutTiepTuc = true,
        };
    }

    public KetQuaAtmViewModel HuyGiaoDich()
    {
        return new KetQuaAtmViewModel
        {
            TieuDe = "Đã hủy",
            ThongBao = "Giao dịch đã bị hủy",
            HienNutTiepTuc = true,
        };
    }

    public KetQuaAtmViewModel KetThucPhien()
    {
        var bienLaiJson = Session.GetString(AtmSessionKeys.BienLai);
        BienLaiAtmVm? bienLai = null;
        if (!string.IsNullOrEmpty(bienLaiJson))
        {
            var bl = System.Text.Json.JsonSerializer.Deserialize<BienLaiAtm>(bienLaiJson);
            if (bl != null) bienLai = MapBienLai(bl);
        }

        XoaPhien();
        return new KetQuaAtmViewModel
        {
            TieuDe = "Cảm ơn quý khách",
            ThongBao = "Đã in hóa đơn và trả thẻ. Chúc quý khách một ngày tốt lành!",
            BienLai = bienLai,
            HienNutTiepTuc = false,
        };
    }

    private static BienLaiAtm TaoBienLai(string soThe, string loai, decimal? soTienRut, decimal soDuSau) =>
        new()
        {
            SoThe = soThe,
            LoaiGiaoDich = loai,
            SoTienRut = soTienRut,
            SoDuSauGiaoDich = soDuSau,
            ThoiGian = DateTime.Now,
        };

    private void LuuBienLai(BienLaiAtm bl) =>
        Session.SetString(AtmSessionKeys.BienLai, System.Text.Json.JsonSerializer.Serialize(bl));

    private static BienLaiAtmVm MapBienLai(BienLaiAtm bl) => new()
    {
        SoThe = bl.SoThe,
        LoaiGiaoDich = bl.LoaiGiaoDich,
        SoTienRut = bl.SoTienRut,
        SoDuSauGiaoDich = bl.SoDuSauGiaoDich,
        ThoiGian = bl.ThoiGian,
    };
}