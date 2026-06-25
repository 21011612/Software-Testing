using System.Text.Json;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services.User;

public class CartService
{
    private const string SessionKey = "GioHang";
    private readonly IHttpContextAccessor _http;

    public CartService(IHttpContextAccessor http) => _http = http;

    public List<GioHangItem> GetItems()
    {
        var session = _http.HttpContext?.Session;
        if (session == null) return new List<GioHangItem>();
        var json = session.GetString(SessionKey);
        if (string.IsNullOrEmpty(json)) return new List<GioHangItem>();
        return JsonSerializer.Deserialize<List<GioHangItem>>(json) ?? new List<GioHangItem>();
    }

    public void SaveItems(List<GioHangItem> items)
    {
        var session = _http.HttpContext?.Session;
        if (session == null) return;
        session.SetString(SessionKey, JsonSerializer.Serialize(items));
    }

    public void Add(GioHangItem item)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.MaSP == item.MaSP && i.GhiChu == item.GhiChu);
        if (existing != null)
            existing.SoLuong += item.SoLuong;
        else
            items.Add(item);
        SaveItems(items);
    }

    public void Update(int maSp, int soLuong)
    {
        var items = GetItems();
        var item = items.FirstOrDefault(i => i.MaSP == maSp);
        if (item == null) return;
        if (soLuong <= 0)
            items.Remove(item);
        else
            item.SoLuong = Math.Min(soLuong, 99);
        SaveItems(items);
    }

    public void ChangeQuantity(int maSp, int delta) =>
        Update(maSp, (GetItems().FirstOrDefault(i => i.MaSP == maSp)?.SoLuong ?? 0) + delta);

    public void Remove(int maSp)
    {
        var items = GetItems().Where(i => i.MaSP != maSp).ToList();
        SaveItems(items);
    }

    public void Clear() => SaveItems(new List<GioHangItem>());

    public decimal TongTien() => GetItems().Sum(i => i.ThanhTien);
    public int SoMon() => GetItems().Sum(i => i.SoLuong);
}