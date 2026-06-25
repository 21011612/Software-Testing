using Microsoft.AspNetCore.Http;
using QuanLyQuanCafe.Services.User;
using QuanLyQuanCafe.ViewModels.User;
using System.Threading;

namespace QuanLyQuanCafe.Tests;

public class CartServiceTests
{
    private sealed class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]?> _store = new();
        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[]? value)
        {
            if (_store.TryGetValue(key, out byte[]? stored)) { value = stored; return true; }
            value = null;
            return false;
        }
    }

    private static CartService CreateCart(bool withSession = true)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        if (withSession)
            accessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { Session = new FakeSession() });
        else
            accessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        return new CartService(accessor.Object);
    }

    [Fact]
    public void GetItems_NoSession_ShouldReturnEmptyList() =>
        Assert.Empty(CreateCart(withSession: false).GetItems());

    [Fact]
    public void Add_NewItem_ShouldAddToCart()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, TenSP = "Cà phê sữa", DonGia = 35000, SoLuong = 1 });
        Assert.Single(cart.GetItems());
        Assert.Equal(1, cart.SoMon());
    }

    [Fact]
    public void Add_SameProductSameNote_ShouldMergeQuantity()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 35000, SoLuong = 1, GhiChu = "Ít đá" });
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 35000, SoLuong = 2, GhiChu = "Ít đá" });
        Assert.Equal(3, cart.GetItems().First().SoLuong);
    }

    [Fact]
    public void Add_SameProductDifferentNote_ShouldCreateSeparateLines()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 35000, SoLuong = 1, GhiChu = "Ít đá" });
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 35000, SoLuong = 1, GhiChu = "Nhiều đá" });
        Assert.Equal(2, cart.GetItems().Count);
    }

    [Fact]
    public void Update_QuantityToZero_ShouldRemoveItem()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 5, DonGia = 25000, SoLuong = 2 });
        cart.Update(5, 0);
        Assert.Empty(cart.GetItems());
    }

    [Fact]
    public void Update_QuantityAbove99_ShouldCapAt99()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 3, DonGia = 10000, SoLuong = 1 });
        cart.Update(3, 150);
        Assert.Equal(99, cart.GetItems().First().SoLuong);
    }

    [Fact]
    public void ChangeQuantity_Decrease_ShouldReduceQuantity()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 10, DonGia = 45000, SoLuong = 5 });
        cart.ChangeQuantity(10, -2);
        Assert.Equal(3, cart.GetItems().First().SoLuong);
    }

    [Fact]
    public void Remove_ShouldRemoveOnlyMatchingProduct()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 30000, SoLuong = 1 });
        cart.Add(new GioHangItem { MaSP = 2, DonGia = 40000, SoLuong = 1 });
        cart.Remove(1);
        Assert.Single(cart.GetItems());
        Assert.Equal(2, cart.GetItems().First().MaSP);
    }

    [Fact]
    public void TongTien_ShouldSumAllItems()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 30000, SoLuong = 2 });
        cart.Add(new GioHangItem { MaSP = 2, DonGia = 40000, SoLuong = 1 });
        Assert.Equal(100000, cart.TongTien());
    }

    [Fact]
    public void Clear_ShouldResetCart()
    {
        var cart = CreateCart();
        cart.Add(new GioHangItem { MaSP = 1, DonGia = 25000, SoLuong = 3 });
        cart.Clear();
        Assert.Empty(cart.GetItems());
        Assert.Equal(0, cart.SoMon());
        Assert.Equal(0, cart.TongTien());
    }
}