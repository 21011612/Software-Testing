namespace QuanLyQuanCafe.ViewModels.Admin;

public class ThongKeViewModel
{
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }

    public List<DoanhThuTheoNgayRow> TheoNgay { get; set; } = [];
    public ThongKeTongKetRow? TongKet { get; set; }

    public List<TopSanPhamRow> TopSanPham { get; set; } = [];
    public List<NhanLabelRow> TheoLoaiDon { get; set; } = [];
    public List<NhanLabelRow> TheoPhuongThuc { get; set; } = [];
    public List<NhanLabelRow> TheoLoaiSanPham { get; set; } = [];
}

public class DoanhThuTheoNgayRow
{
    public DateTime Ngay { get; set; }
    public int SoDon { get; set; }
    public decimal DoanhThuGoc { get; set; }
    public decimal TongGiam { get; set; }
    public decimal DoanhThuThuc { get; set; }
    public decimal TrungBinhDon { get; set; }
}

public class ThongKeTongKetRow
{
    public int TongSoDon { get; set; }
    public decimal TongDoanhThuGoc { get; set; }
    public decimal TongGiamGia { get; set; }
    public decimal TongDoanhThuThuc { get; set; }
}

public class TopSanPhamRow
{
    public string TenSP { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DoanhThu { get; set; }
}

public class NhanLabelRow
{
    public string Nhan { get; set; } = "";
    public decimal GiaTri { get; set; }
    public int SoLuong { get; set; }
}

public class AdminDashboardViewModel
{
    public int TongSanPham { get; set; }
    public int BanDangPhucVu { get; set; }
    public int HoaDonChoThanhToan { get; set; }
    public int KhachHang { get; set; }
    public decimal DoanhThuHomNay { get; set; }
    public int DonHomNay { get; set; }
    public int DatBanSapToi { get; set; }
    public ThongKeViewModel ThongKe { get; set; } = new();
}