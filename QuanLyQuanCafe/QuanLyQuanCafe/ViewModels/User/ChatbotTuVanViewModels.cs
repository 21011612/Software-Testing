namespace QuanLyQuanCafe.ViewModels.User;

public class ChatbotTuVanViewModel
{
    public string DiaChi { get; set; } = "123 Nguyễn Tất Thành, Sơn Trà, Đà Nẵng";
    public string GioMoCua { get; set; } = "7:00 – 22:00 (hàng ngày)";
    public string DienThoai { get; set; } = "0236 3xxx xxx";
    public int TongSoMon { get; set; }
    public string AnhQuan { get; set; } = "/images/Ca_phe_latte_art.jpg";
    public List<ChatbotLoaiMonViewModel> LoaiMon { get; set; } = [];
    public List<ChatbotKhuyenMaiViewModel> KhuyenMai { get; set; } = [];
}

public class ChatbotLoaiMonViewModel
{
    public int MaLoai { get; set; }
    public string TenLoai { get; set; } = "";
    public string? MoTaLoai { get; set; }
    public string? AnhLoai { get; set; }
    public List<ChatbotMonViewModel> Mon { get; set; } = [];
}

public class ChatbotMonViewModel
{
    public int MaSp { get; set; }
    public string Ten { get; set; } = "";
    public decimal Gia { get; set; }
    public string DonVi { get; set; } = "Ly";
    public string? MoTa { get; set; }
    public string TenLoai { get; set; } = "";
    public string HinhAnh { get; set; } = "/images/Ca_phe_sua_da.jpg";
}

public class ChatbotKhuyenMaiViewModel
{
    public string Ten { get; set; } = "";
    public string MoTaGiam { get; set; } = "";
    public string HetHan { get; set; } = "";
}