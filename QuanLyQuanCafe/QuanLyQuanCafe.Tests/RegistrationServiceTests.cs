using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Tests;

public class RegistrationServiceTests : IDisposable
{
    private readonly TooruCoffeeDbContext _db;
    private readonly RegistrationService _service;

    public RegistrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<TooruCoffeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TooruCoffeeDbContext(options);
        _service = new RegistrationService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task DangKyAsync_ValidData_ShouldReturnSuccess()
    {
        var model = CreateModel("user01", "0911111111");

        var (success, error) = await _service.DangKyAsync(model);

        Assert.True(success);
        Assert.Null(error);
    }

    [Fact]
    public async Task DangKyAsync_ValidData_ShouldCreateKhachHangAndTaiKhoan()
    {
        var model = CreateModel("user02", "0922222222", hoTen: "Trần Thị B");

        await _service.DangKyAsync(model);

        var kh = await _db.KhachHangs.FirstAsync(k => k.SDT == "0922222222");
        var tk = await _db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "user02");

        Assert.Equal("Trần Thị B", kh.HoTen);
        Assert.Equal(kh.MaKH, tk.MaKH);
        Assert.Equal("KhachHang", tk.VaiTro);
    }

    [Fact]
    public async Task DangKyAsync_ShouldReturnError_WhenTenDangNhapExists()
    {
        await SeedUserAsync("dupuser", "0900000001");

        var (success, error) = await _service.DangKyAsync(CreateModel("dupuser", "0900000002"));

        Assert.False(success);
        Assert.Equal("Tên đăng nhập đã tồn tại.", error);
    }

    [Fact]
    public async Task DangKyAsync_ShouldReturnError_WhenSdtExists()
    {
        await SeedUserAsync("user_a", "0933333333");

        var (success, error) = await _service.DangKyAsync(CreateModel("user_b", "0933333333"));

        Assert.False(success);
        Assert.Equal("Số điện thoại đã được đăng ký.", error);
    }

    [Fact]
    public async Task DangKyAsync_ShouldHashPasswordWithBcrypt()
    {
        await _service.DangKyAsync(CreateModel("hashuser", "0944444444", matKhau: "Secret@2026"));

        var tk = await _db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "hashuser");

        Assert.StartsWith("$2", tk.MatKhauHash);
        Assert.True(PasswordHelper.Verify("Secret@2026", tk.MatKhauHash));
    }

    [Fact]
    public async Task DangKyAsync_ShouldSetDefaultKhachHangFields()
    {
        await _service.DangKyAsync(CreateModel("defaultuser", "0955555555"));

        var kh = await _db.KhachHangs.FirstAsync(k => k.SDT == "0955555555");

        Assert.Equal(0, kh.DiemTichLuy);
        Assert.Equal("Thường", kh.HangThanhVien);
        Assert.True(kh.NgayTao <= DateTime.Now);
    }

    [Fact]
    public async Task DangKyAsync_ShouldSaveOptionalEmailAndDiaChi()
    {
        var model = CreateModel("optuser", "0966666666");
        model.Email = "opt@mail.com";
        model.DiaChi = "123 Nha Trang";

        await _service.DangKyAsync(model);

        var kh = await _db.KhachHangs.FirstAsync(k => k.SDT == "0966666666");
        Assert.Equal("opt@mail.com", kh.Email);
        Assert.Equal("123 Nha Trang", kh.DiaChi);
    }

    [Fact]
    public async Task DangKyAsync_ShouldActivateTaiKhoan()
    {
        await _service.DangKyAsync(CreateModel("activeuser", "0977777777"));

        var tk = await _db.TaiKhoans.FirstAsync(t => t.TenDangNhap == "activeuser");

        Assert.True(tk.TrangThai);
        Assert.True(tk.NgayTao <= DateTime.Now);
    }

    [Fact]
    public async Task DangKyAsync_TwoDifferentUsers_ShouldBothSucceed()
    {
        var r1 = await _service.DangKyAsync(CreateModel("multi1", "0981111111"));
        var r2 = await _service.DangKyAsync(CreateModel("multi2", "0982222222"));

        Assert.True(r1.Success);
        Assert.True(r2.Success);
        Assert.Equal(2, await _db.TaiKhoans.CountAsync());
    }

    [Fact]
    public async Task DangKyAsync_ShouldLinkTaiKhoanOnlyToKhachHang()
    {
        await _service.DangKyAsync(CreateModel("linkuser", "0999999999"));

        var tk = await _db.TaiKhoans.Include(t => t.KhachHang).FirstAsync(t => t.TenDangNhap == "linkuser");

        Assert.NotNull(tk.KhachHang);
        Assert.Null(tk.MaNV);
    }

    private static DangKyViewModel CreateModel(string tenDangNhap, string sdt, string hoTen = "Test User", string matKhau = "Password123!")
        => new()
        {
            HoTen = hoTen,
            SDT = sdt,
            TenDangNhap = tenDangNhap,
            MatKhau = matKhau,
            XacNhanMatKhau = matKhau
        };

    private async Task SeedUserAsync(string tenDangNhap, string sdt)
    {
        var kh = new KhachHang { HoTen = "Existing", SDT = sdt, HangThanhVien = "Thường", NgayTao = DateTime.Now };
        _db.KhachHangs.Add(kh);
        await _db.SaveChangesAsync();

        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = tenDangNhap,
            MatKhauHash = PasswordHelper.Hash("Password123!"),
            VaiTro = "KhachHang",
            MaKH = kh.MaKH,
            TrangThai = true,
            NgayTao = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }
}