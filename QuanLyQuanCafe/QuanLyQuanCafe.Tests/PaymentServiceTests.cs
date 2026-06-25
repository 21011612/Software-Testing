using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Services.User;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Tests;

public class PaymentServiceTests : IDisposable
{
    private readonly TooruCoffeeDbContext _db;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<TooruCoffeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TooruCoffeeDbContext(options);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void PhuongThucThanhToan_ShouldHaveSevenMethods() =>
        Assert.Equal(7, PaymentService.PhuongThucThanhToan.Length);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainTienMat() =>
        Assert.Contains("Tiền mặt", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainThe() =>
        Assert.Contains("Thẻ", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainChuyenKhoan() =>
        Assert.Contains("Chuyển khoản", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainMomo() =>
        Assert.Contains("Momo", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainZaloPay() =>
        Assert.Contains("ZaloPay", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainShopeePay() =>
        Assert.Contains("ShopeePay", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldContainKhac() =>
        Assert.Contains("Khác", PaymentService.PhuongThucThanhToan);

    [Fact]
    public void PhuongThucThanhToan_ShouldMatchSqlConstraintValues()
    {
        var expected = new[] { "Tiền mặt", "Thẻ", "Chuyển khoản", "Momo", "ZaloPay", "ShopeePay", "Khác" };
        Assert.Equal(expected, PaymentService.PhuongThucThanhToan);
    }

    [Fact]
    public async Task ThanhToanAsync_InMemoryDb_ShouldThrowRelationalError()
    {
        var service = new PaymentService(_db);
        var model = new ThanhToanViewModel { MaHD = 1, PhuongThuc = "Tiền mặt", TongThanhToan = 100000 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ThanhToanAsync(model, 1));
    }
}