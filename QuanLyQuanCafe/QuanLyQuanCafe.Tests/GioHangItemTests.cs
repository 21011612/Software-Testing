using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Tests;

public class GioHangItemTests
{
    [Fact]
    public void ThanhTien_SingleItem_ShouldEqualDonGia() =>
        Assert.Equal(39000, new GioHangItem { DonGia = 39000, SoLuong = 1 }.ThanhTien);

    [Fact]
    public void ThanhTien_TwoQuantity_ShouldMultiply() =>
        Assert.Equal(70000, new GioHangItem { DonGia = 35000, SoLuong = 2 }.ThanhTien);

    [Fact]
    public void ThanhTien_ThreeQuantity_ShouldMultiply() =>
        Assert.Equal(75000, new GioHangItem { DonGia = 25000, SoLuong = 3 }.ThanhTien);

    [Fact]
    public void ThanhTien_ZeroQuantity_ShouldBeZero() =>
        Assert.Equal(0, new GioHangItem { DonGia = 50000, SoLuong = 0 }.ThanhTien);

    [Fact]
    public void ThanhTien_DecimalPrice_ShouldCalculateCorrectly() =>
        Assert.Equal(147000, new GioHangItem { DonGia = 49000, SoLuong = 3 }.ThanhTien);

    [Fact]
    public void DefaultValues_ShouldBeSet()
    {
        var item = new GioHangItem();
        Assert.Equal(1, item.SoLuong);
        Assert.Equal("", item.TenSP);
        Assert.Equal("", item.HinhAnh);
    }

    [Fact]
    public void Properties_ShouldStoreProductInfo()
    {
        var item = new GioHangItem
        {
            MaSP = 12,
            TenSP = "Americano Tiki",
            HinhAnh = "images/Americano_Tiki.jpg",
            DonGia = 49000,
            SoLuong = 2,
            GhiChu = "Ít đá"
        };
        Assert.Equal(12, item.MaSP);
        Assert.Equal("Americano Tiki", item.TenSP);
        Assert.Equal("Ít đá", item.GhiChu);
        Assert.Equal(98000, item.ThanhTien);
    }

    [Fact]
    public void ThanhTien_LargeOrder_ShouldWork() =>
        Assert.Equal(990000, new GioHangItem { DonGia = 33000, SoLuong = 30 }.ThanhTien);

    [Fact]
    public void ThanhTien_FoodItem_ShouldWork() =>
        Assert.Equal(75000, new GioHangItem { DonGia = 75000, SoLuong = 1, TenSP = "Cà kho tộ" }.ThanhTien);

    [Fact]
    public void GhiChu_CanBeNull()
    {
        var item = new GioHangItem { MaSP = 1, DonGia = 10000, SoLuong = 1, GhiChu = null };
        Assert.Null(item.GhiChu);
        Assert.Equal(10000, item.ThanhTien);
    }
}