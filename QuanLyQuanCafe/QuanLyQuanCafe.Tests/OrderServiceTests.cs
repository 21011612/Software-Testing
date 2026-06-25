using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services.User;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TooruCoffeeDbContext _db;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<TooruCoffeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TooruCoffeeDbContext(options);
        _db.SanPhams.AddRange(
            new SanPham { MaSP = 1, TenSP = "Cà phê sữa", Gia = 35000, HinhAnh = "images/cf.jpg", DonViTinh = "Ly", TrangThai = true },
            new SanPham { MaSP = 2, TenSP = "Trà đào", Gia = 42000, HinhAnh = "images/tra.jpg", DonViTinh = "Ly", TrangThai = true }
        );
        _db.SaveChanges();
        _service = new OrderService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task TaoHoaDonAsync_EmptyCart_ShouldThrow()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.TaoHoaDonAsync(new DatHangViewModel { LoaiDon = "Tại quán" }, 1, 10, []));
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_NotFound_ShouldReturnNull()
    {
        var result = await _service.GetHoaDonThanhToanAsync(9999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_ShouldMapHeaderFields()
    {
        await SeedHoaDonAsync(1, 150000, 10000, 140000, "Chờ thanh toán");

        var result = await _service.GetHoaDonThanhToanAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.MaHD);
        Assert.Equal(150000, result.TongTienTruocGiam);
        Assert.Equal(10000, result.GiamGia);
        Assert.Equal(140000, result.TongThanhToan);
        Assert.Equal("Chờ thanh toán", result.TrangThai);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_ShouldMapChiTietSanPham()
    {
        await SeedHoaDonAsync(2, 70000, 0, 70000, "Chờ thanh toán", (1, 2, 35000, "Ít đá"));

        var result = await _service.GetHoaDonThanhToanAsync(2);

        Assert.NotNull(result);
        Assert.Single(result!.ChiTiet);
        Assert.Equal("Cà phê sữa", result.ChiTiet[0].TenSP);
        Assert.Equal(2, result.ChiTiet[0].SoLuong);
        Assert.Equal(35000, result.ChiTiet[0].DonGia);
        Assert.Equal("Ít đá", result.ChiTiet[0].GhiChu);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_MultipleItems_ShouldReturnAll()
    {
        await SeedHoaDonAsync(3, 119000, 0, 119000, "Chờ thanh toán",
            (1, 1, 35000, null),
            (2, 2, 42000, "Nhiều đá"));

        var result = await _service.GetHoaDonThanhToanAsync(3);

        Assert.NotNull(result);
        Assert.Equal(2, result!.ChiTiet.Count);
        Assert.Equal(119000, result.TongThanhToan);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_PaidOrder_ShouldReturnStatus()
    {
        await SeedHoaDonAsync(4, 99000, 20000, 79000, "Đã thanh toán");

        var result = await _service.GetHoaDonThanhToanAsync(4);

        Assert.Equal("Đã thanh toán", result!.TrangThai);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_ShouldIncludeHinhAnh()
    {
        await SeedHoaDonAsync(5, 42000, 0, 42000, "Chờ thanh toán", (2, 1, 42000, null));

        var result = await _service.GetHoaDonThanhToanAsync(5);

        Assert.Equal("images/tra.jpg", result!.ChiTiet[0].HinhAnh);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_EmptyChiTiet_ShouldReturnEmptyList()
    {
        _db.HoaDons.Add(new HoaDon
        {
            MaHD = 6,
            TongThanhToan = 0,
            TongTienTruocGiam = 0,
            TrangThai = "Chờ thanh toán",
            NgayLap = DateTime.Now
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetHoaDonThanhToanAsync(6);

        Assert.NotNull(result);
        Assert.Empty(result!.ChiTiet);
    }

    [Fact]
    public async Task GetHoaDonThanhToanAsync_MissingSanPham_ShouldUseEmptyName()
    {
        _db.HoaDons.Add(new HoaDon { MaHD = 7, TongThanhToan = 10000, TongTienTruocGiam = 10000, TrangThai = "Chờ thanh toán" });
        _db.ChiTietHoaDons.Add(new ChiTietHoaDon { MaHD = 7, MaSP = 999, SoLuong = 1, DonGia = 10000 });
        await _db.SaveChangesAsync();

        var result = await _service.GetHoaDonThanhToanAsync(7);

        Assert.Equal("", result!.ChiTiet[0].TenSP);
    }

    private async Task SeedHoaDonAsync(int maHd, decimal truocGiam, decimal giam, decimal thanhToan, string trangThai,
        params (int maSp, int soLuong, decimal donGia, string? ghiChu)[] chiTiet)
    {
        _db.HoaDons.Add(new HoaDon
        {
            MaHD = maHd,
            TongTienTruocGiam = truocGiam,
            GiamGia = giam,
            TongThanhToan = thanhToan,
            TrangThai = trangThai,
            NgayLap = DateTime.Now
        });
        foreach (var ct in chiTiet)
        {
            _db.ChiTietHoaDons.Add(new ChiTietHoaDon
            {
                MaHD = maHd,
                MaSP = ct.maSp,
                SoLuong = ct.soLuong,
                DonGia = ct.donGia,
                GhiChu = ct.ghiChu
            });
        }
        await _db.SaveChangesAsync();
    }
}