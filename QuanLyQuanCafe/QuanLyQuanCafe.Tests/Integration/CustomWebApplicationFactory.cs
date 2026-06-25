using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;

namespace QuanLyQuanCafe.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"IntegrationTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<TooruCoffeeDbContext>(services);

            services.AddDbContext<TooruCoffeeDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IAntiforgery>();
            services.AddSingleton<IAntiforgery, AlwaysValidAntiforgery>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TooruCoffeeDbContext>();
        db.Database.EnsureCreated();
        SeedTestData(db);

        return host;
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services) where TContext : DbContext
    {
        var descriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                d.ServiceType == typeof(TContext) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>) &&
                 d.ServiceType.GenericTypeArguments[0] == typeof(TContext)))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }

    private static void SeedTestData(TooruCoffeeDbContext db)
    {
        if (db.SanPhams.Any()) return;

        db.LoaiSanPhams.AddRange(
            new LoaiSanPham { MaLoai = 1, TenLoai = "Cà phê", TrangThai = true, ThuTuHienThi = 1 },
            new LoaiSanPham { MaLoai = 2, TenLoai = "Trà", TrangThai = true, ThuTuHienThi = 2 }
        );
        db.SaveChanges();

        db.SanPhams.AddRange(
            new SanPham { MaSP = 1, TenSP = "Cà phê sữa đá", Gia = 35000, DonViTinh = "Ly", TrangThai = true, MaLoai = 1, HinhAnh = "images/Ca_phe_sua_da.jpg" },
            new SanPham { MaSP = 2, TenSP = "Cà phê đen đá", Gia = 30000, DonViTinh = "Ly", TrangThai = true, MaLoai = 1, HinhAnh = "images/Ca_phe_da_chen.jpg" },
            new SanPham { MaSP = 3, TenSP = "Trà đào cam sả", Gia = 45000, DonViTinh = "Ly", TrangThai = true, MaLoai = 2, HinhAnh = "images/Tra_cam_da_cocktail.jpg" }
        );

        db.Bans.AddRange(
            new Ban { MaBan = 1, TenBan = "B01", TrangThai = "Trống" },
            new Ban { MaBan = 2, TenBan = "B02", TrangThai = "Trống" },
            new Ban { MaBan = 3, TenBan = "B03", TrangThai = "Đang phục vụ" }
        );

        db.KhuyenMais.Add(new KhuyenMai
        {
            MaKM = 1,
            TenKM = "Giảm 10% cho đơn trên 100k",
            LoaiGiam = "PhanTram",
            GiaTriGiam = 10,
            TrangThai = true,
            NgayBatDau = DateTime.Today.AddDays(-10),
            NgayKetThuc = DateTime.Today.AddDays(10)
        });

        db.NhanViens.Add(new NhanVien
        {
            MaNV = 1,
            HoTen = "Staff Test",
            SDT = "0900000001",
            ChucVu = "Phục vụ",
            TrangThai = true,
            NgayVaoLam = DateTime.Now
        });

        var kh = new KhachHang
        {
            MaKH = 1,
            HoTen = "Test User",
            SDT = "0901234567",
            Email = "test@tooru.com",
            HangThanhVien = "Thường",
            NgayTao = DateTime.Now
        };
        db.KhachHangs.Add(kh);

        db.TaiKhoans.Add(new TaiKhoan
        {
            MaTK = 1,
            TenDangNhap = "testuser",
            MatKhauHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
            VaiTro = "KhachHang",
            MaKH = kh.MaKH,
            TrangThai = true,
            NgayTao = DateTime.Now
        });

        db.SaveChanges();
    }
}