using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Models;

namespace QuanLyQuanCafe.Services;

/// <summary>
/// Đồng bộ mật khẩu tài khoản demo (plain text trong SQL script → BCrypt khi chạy app).
/// </summary>
public class DatabaseSeedService
{
    private readonly TooruCoffeeDbContext _db;
    private readonly ILogger<DatabaseSeedService> _logger;

    private static readonly Dictionary<string, (string Password, string VaiTro, int? MaNV, int? MaKH)> SeedAccounts = new()
    {
        ["admin"] = ("admin@Tooru2026!", "Admin", null, null),
        ["barista01"] = ("barista@123", "NhanVien", 2, null),
        ["thungan01"] = ("thungan@123", "NhanVien", 4, null),
        ["phucvu01"] = ("phucvu@123", "NhanVien", 3, null),
        ["khachhang1"] = ("khach@123", "KhachHang", null, 1),
    };

    public DatabaseSeedService(TooruCoffeeDbContext db, ILogger<DatabaseSeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task EnsureTaiKhoanPasswordsAsync()
    {
        try
        {
            if (!await _db.Database.CanConnectAsync())
            {
                _logger.LogWarning("Không kết nối được CSDL TooruCoffee — bỏ qua seed mật khẩu.");
                return;
            }

            foreach (var (tenDangNhap, info) in SeedAccounts)
            {
                var tk = await _db.TaiKhoans
                    .FirstOrDefaultAsync(t => t.TenDangNhap.ToLower() == tenDangNhap.ToLower());

                if (tk == null)
                {
                    _logger.LogWarning("Thiếu tài khoản {User} — chạy script SQL ThietKeCSDL_QuanLyQuanCaPhe.sql", tenDangNhap);
                    continue;
                }

                if (!PasswordHelper.Verify(info.Password, tk.MatKhauHash))
                {
                    tk.MatKhauHash = PasswordHelper.Hash(info.Password);
                    tk.TrangThai = true;
                    _logger.LogInformation("Đã cập nhật mật khẩu BCrypt cho {User}", tenDangNhap);
                }
                else if (!tk.MatKhauHash.StartsWith("$2", StringComparison.Ordinal))
                {
                    tk.MatKhauHash = PasswordHelper.Hash(info.Password);
                    _logger.LogInformation("Đã chuyển mật khẩu plain text sang BCrypt cho {User}", tenDangNhap);
                }
            }

            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi đồng bộ mật khẩu tài khoản");
        }
    }

    public async Task EnsureDanhGiaBinhLuanTableAsync()
    {
        try
        {
            if (!await _db.Database.CanConnectAsync()) return;

            await _db.Database.ExecuteSqlRawAsync("""
                IF OBJECT_ID(N'dbo.DanhGiaBinhLuan', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.DanhGiaBinhLuan (
                        MaDG INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        MaSP INT NOT NULL,
                        MaKH INT NULL,
                        HoTenHienThi NVARCHAR(100) NOT NULL,
                        SoSao TINYINT NOT NULL,
                        NoiDung NVARCHAR(500) NOT NULL,
                        NgayTao DATETIME NOT NULL CONSTRAINT DF_DanhGia_NgayTao DEFAULT (GETDATE()),
                        TrangThai BIT NOT NULL CONSTRAINT DF_DanhGia_TrangThai DEFAULT (1),
                        CONSTRAINT CK_DanhGia_SoSao CHECK (SoSao BETWEEN 1 AND 5),
                        CONSTRAINT FK_DanhGia_SanPham FOREIGN KEY (MaSP) REFERENCES dbo.SanPham(MaSP) ON DELETE CASCADE,
                        CONSTRAINT FK_DanhGia_KhachHang FOREIGN KEY (MaKH) REFERENCES dbo.KhachHang(MaKH) ON DELETE SET NULL
                    );
                    CREATE INDEX IX_DanhGia_MaSP ON dbo.DanhGiaBinhLuan(MaSP);
                    CREATE UNIQUE INDEX UQ_DanhGia_MaSP_MaKH ON dbo.DanhGiaBinhLuan(MaSP, MaKH) WHERE MaKH IS NOT NULL;
                END
                """);
            _logger.LogInformation("Đã kiểm tra bảng DanhGiaBinhLuan.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tạo được bảng DanhGiaBinhLuan — chạy Scripts/TaoBangDanhGiaBinhLuan.sql thủ công.");
        }
    }
}