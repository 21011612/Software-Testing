using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services.User;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Tests;

public class DanhGiaServiceTests : IDisposable
{
    private readonly TooruCoffeeDbContext _db;
    private readonly DanhGiaService _service;

    public DanhGiaServiceTests()
    {
        var options = new DbContextOptionsBuilder<TooruCoffeeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TooruCoffeeDbContext(options);
        _db.SanPhams.AddRange(
            new SanPham { MaSP = 1, TenSP = "Cà phê sữa đá", TrangThai = true, MaLoai = 1 },
            new SanPham { MaSP = 2, TenSP = "Trà đào", TrangThai = false, MaLoai = 2 }
        );
        _db.SaveChanges();
        _service = new DanhGiaService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task LuuDanhGiaAsync_ValidGuestReview_ShouldSave()
    {
        var model = new DanhGiaFormViewModel { MaSP = 1, SoSao = 5, NoiDung = "Rất ngon, sẽ quay lại!", HoTenHienThi = "Khách A" };

        var (success, error) = await _service.LuuDanhGiaAsync(model, null, null);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(1, await _db.DanhGiaBinhLuans.CountAsync());
    }

    [Fact]
    public async Task LuuDanhGiaAsync_ValidMemberReview_ShouldSaveWithMaKh()
    {
        var model = new DanhGiaFormViewModel { MaSP = 1, SoSao = 4, NoiDung = "Khá ngon, giá hợp lý", HoTenHienThi = "KH B" };

        var (success, _) = await _service.LuuDanhGiaAsync(model, 5, "KH B");

        var dg = await _db.DanhGiaBinhLuans.FirstAsync();
        Assert.True(success);
        Assert.Equal(5, dg.MaKH);
    }

    [Fact]
    public async Task LuuDanhGiaAsync_InvalidStars_ShouldFail()
    {
        var (success, error) = await _service.LuuDanhGiaAsync(
            new DanhGiaFormViewModel { MaSP = 1, SoSao = 0, NoiDung = "Tệ lắm", HoTenHienThi = "T" }, null, "T");

        Assert.False(success);
        Assert.Contains("1 đến 5", error);
    }

    [Fact]
    public async Task LuuDanhGiaAsync_TooShortComment_ShouldFail()
    {
        var (success, error) = await _service.LuuDanhGiaAsync(
            new DanhGiaFormViewModel { MaSP = 1, SoSao = 4, NoiDung = "Ng", HoTenHienThi = "T" }, null, "T");

        Assert.False(success);
        Assert.Contains("tối thiểu 5 ký tự", error);
    }

    [Fact]
    public async Task LuuDanhGiaAsync_InactiveProduct_ShouldFail()
    {
        var (success, error) = await _service.LuuDanhGiaAsync(
            new DanhGiaFormViewModel { MaSP = 2, SoSao = 3, NoiDung = "Không bán", HoTenHienThi = "T" }, null, "T");

        Assert.False(success);
        Assert.Equal("Sản phẩm không tồn tại.", error);
    }

    [Fact]
    public async Task LuuDanhGiaAsync_MissingName_ShouldFail()
    {
        var (success, error) = await _service.LuuDanhGiaAsync(
            new DanhGiaFormViewModel { MaSP = 1, SoSao = 5, NoiDung = "Ngon quá đi", HoTenHienThi = "" }, null, null);

        Assert.False(success);
        Assert.Contains("họ tên", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LuuDanhGiaAsync_ExistingMemberReview_ShouldUpdate()
    {
        _db.DanhGiaBinhLuans.Add(new DanhGiaBinhLuan { MaSP = 1, MaKH = 9, SoSao = 3, NoiDung = "Bình thường", HoTenHienThi = "Cũ", TrangThai = true });
        await _db.SaveChangesAsync();

        var (success, _) = await _service.LuuDanhGiaAsync(
            new DanhGiaFormViewModel { MaSP = 1, SoSao = 5, NoiDung = "Lần sau ngon hơn", HoTenHienThi = "Mới" }, 9, "Mới");

        var dg = await _db.DanhGiaBinhLuans.FirstAsync(d => d.MaKH == 9);
        Assert.True(success);
        Assert.Equal((byte)5, dg.SoSao);
        Assert.Equal("Lần sau ngon hơn", dg.NoiDung);
        Assert.Equal(1, await _db.DanhGiaBinhLuans.CountAsync());
    }

    [Fact]
    public async Task LayDanhSachHienThiAsync_ShouldFilterInactive()
    {
        _db.DanhGiaBinhLuans.AddRange(
            new DanhGiaBinhLuan { MaSP = 1, SoSao = 5, NoiDung = "Tốt", HoTenHienThi = "A", TrangThai = true, NgayTao = DateTime.Now },
            new DanhGiaBinhLuan { MaSP = 1, SoSao = 1, NoiDung = "Ẩn", HoTenHienThi = "B", TrangThai = false, NgayTao = DateTime.Now }
        );
        await _db.SaveChangesAsync();

        var list = await _service.LayDanhSachHienThiAsync(1);
        Assert.Single(list);
    }

    [Fact]
    public async Task LayTongHopNhieuAsync_ShouldReturnDictionaryByMaSp()
    {
        _db.DanhGiaBinhLuans.AddRange(
            new DanhGiaBinhLuan { MaSP = 1, SoSao = 5, TrangThai = true },
            new DanhGiaBinhLuan { MaSP = 1, SoSao = 3, TrangThai = true }
        );
        await _db.SaveChangesAsync();

        var dict = await _service.LayTongHopNhieuAsync([1]);

        Assert.True(dict.ContainsKey(1));
        Assert.Equal(4.0, dict[1].DiemTrungBinh);
        Assert.Equal(2, dict[1].SoDanhGia);
    }

    [Fact]
    public async Task LayDanhGiaCuaKhachAsync_ShouldReturnEntityWhenExists()
    {
        _db.DanhGiaBinhLuans.Add(new DanhGiaBinhLuan { MaSP = 1, MaKH = 7, SoSao = 4, NoiDung = "OK", HoTenHienThi = "KH", TrangThai = true });
        await _db.SaveChangesAsync();

        var result = await _service.LayDanhGiaCuaKhachAsync(1, 7);

        Assert.NotNull(result);
        Assert.Equal((byte)4, result!.SoSao);
    }
}