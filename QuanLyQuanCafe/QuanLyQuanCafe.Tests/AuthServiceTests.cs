using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;

namespace QuanLyQuanCafe.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TooruCoffeeDbContext _db;
    private readonly Mock<IHttpContextAccessor> _httpAccessorMock;
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<IAuthenticationService> _authenticationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<TooruCoffeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TooruCoffeeDbContext(options);

        _httpAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContext = new DefaultHttpContext();
        _authenticationMock = new Mock<IAuthenticationService>();

        _httpContext.RequestServices = new ServiceCollection()
            .AddSingleton(_authenticationMock.Object)
            .BuildServiceProvider();

        _httpAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);
        _authService = new AuthService(_db, _httpAccessorMock.Object);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task DangNhapKhachHangAsync_ValidCredentials_ShouldSucceed()
    {
        await SeedCustomerAsync("kh01", "0901000001", "Pass@123");

        var (success, error) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "kh01",
            MatKhau = "Pass@123"
        });

        Assert.True(success);
        Assert.Null(error);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_ShouldUpdateLanDangNhapCuoi()
    {
        var tk = await SeedCustomerAsync("kh02", "0901000002", "Pass@123");

        await _authService.DangNhapKhachHangAsync(new DangNhapViewModel { TenDangNhap = "kh02", MatKhau = "Pass@123" });

        var updated = await _db.TaiKhoans.FindAsync(tk.MaTK);
        Assert.NotNull(updated?.LanDangNhapCuoi);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_UsernameCaseInsensitive_ShouldSucceed()
    {
        await SeedCustomerAsync("KhachHang", "0901000003", "Pass@123");

        var (success, _) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "  khachhang  ",
            MatKhau = "Pass@123"
        });

        Assert.True(success);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_PlainTextPassword_ShouldSucceed()
    {
        var kh = new KhachHang { HoTen = "Plain", SDT = "0901000004", HangThanhVien = "Thường", NgayTao = DateTime.Now };
        _db.KhachHangs.Add(kh);
        await _db.SaveChangesAsync();
        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = "plainuser",
            MatKhauHash = "plain@123",
            VaiTro = "KhachHang",
            MaKH = kh.MaKH,
            TrangThai = true
        });
        await _db.SaveChangesAsync();

        var (success, _) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "plainuser",
            MatKhau = "plain@123"
        });

        Assert.True(success);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_WrongPassword_ShouldFail()
    {
        await SeedCustomerAsync("wrongpass", "0901000005", "Correct123");

        var (success, error) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "wrongpass",
            MatKhau = "SaiMatKhau"
        });

        Assert.False(success);
        Assert.Contains("mật khẩu", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_LockedAccount_ShouldFail()
    {
        var tk = await SeedCustomerAsync("locked", "0901000006", "Pass@123");
        tk.TrangThai = false;
        await _db.SaveChangesAsync();

        var (success, error) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "locked",
            MatKhau = "Pass@123"
        });

        Assert.False(success);
        Assert.Contains("đã bị khóa", error);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_AdminAccount_ShouldFailWithAdminMessage()
    {
        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = "admin01",
            MatKhauHash = PasswordHelper.Hash("Admin@123"),
            VaiTro = "Admin",
            TrangThai = true
        });
        await _db.SaveChangesAsync();

        var (success, error) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "admin01",
            MatKhau = "Admin@123"
        });

        Assert.False(success);
        Assert.Contains("cổng Quản trị", error);
    }

    [Fact]
    public async Task DangNhapQuanTriAsync_NhanVienAccount_ShouldSucceed()
    {
        var nv = new NhanVien { HoTen = "NV Test", SDT = "0902000001", ChucVu = "Phục vụ", NgayVaoLam = DateTime.Now, TrangThai = true };
        _db.NhanViens.Add(nv);
        await _db.SaveChangesAsync();
        _db.TaiKhoans.Add(new TaiKhoan
        {
            TenDangNhap = "nv01",
            MatKhauHash = PasswordHelper.Hash("Nv@123"),
            VaiTro = "NhanVien",
            MaNV = nv.MaNV,
            TrangThai = true
        });
        await _db.SaveChangesAsync();

        var (success, error) = await _authService.DangNhapQuanTriAsync(new DangNhapViewModel
        {
            TenDangNhap = "nv01",
            MatKhau = "Nv@123"
        });

        Assert.True(success);
        Assert.Null(error);
    }

    [Fact]
    public async Task DangNhapQuanTriAsync_KhachHangAccount_ShouldFail()
    {
        await SeedCustomerAsync("khqt", "0901000007", "Pass@123");

        var (success, error) = await _authService.DangNhapQuanTriAsync(new DangNhapViewModel
        {
            TenDangNhap = "khqt",
            MatKhau = "Pass@123"
        });

        Assert.False(success);
        Assert.Contains("khách hàng", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DangNhapKhachHangAsync_EmptyCredentials_ShouldFail()
    {
        var (success, error) = await _authService.DangNhapKhachHangAsync(new DangNhapViewModel
        {
            TenDangNhap = "",
            MatKhau = ""
        });

        Assert.False(success);
        Assert.Contains("đầy đủ", error);
    }

    private async Task<TaiKhoan> SeedCustomerAsync(string tenDangNhap, string sdt, string password)
    {
        var kh = new KhachHang { HoTen = "KH Test", SDT = sdt, HangThanhVien = "Thường", NgayTao = DateTime.Now };
        _db.KhachHangs.Add(kh);
        await _db.SaveChangesAsync();

        var tk = new TaiKhoan
        {
            TenDangNhap = tenDangNhap,
            MatKhauHash = PasswordHelper.Hash(password),
            VaiTro = "KhachHang",
            MaKH = kh.MaKH,
            TrangThai = true,
            NgayTao = DateTime.Now
        };
        _db.TaiKhoans.Add(tk);
        await _db.SaveChangesAsync();
        return tk;
    }
}