using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Models;

namespace QuanLyQuanCafe.Data;

public class TooruCoffeeDbContext : DbContext
{
    public TooruCoffeeDbContext(DbContextOptions<TooruCoffeeDbContext> options) : base(options) { }

    public DbSet<LoaiSanPham> LoaiSanPhams => Set<LoaiSanPham>();
    public DbSet<SanPham> SanPhams => Set<SanPham>();
    public DbSet<Ban> Bans => Set<Ban>();
    public DbSet<KhachHang> KhachHangs => Set<KhachHang>();
    public DbSet<NhanVien> NhanViens => Set<NhanVien>();
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<KhuyenMai> KhuyenMais => Set<KhuyenMai>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<DatBan> DatBans => Set<DatBan>();
    public DbSet<DanhGiaBinhLuan> DanhGiaBinhLuans => Set<DanhGiaBinhLuan>();
    public DbSet<NhaCungCap> NhaCungCaps => Set<NhaCungCap>();
    public DbSet<NguyenLieu> NguyenLieus => Set<NguyenLieu>();
    public DbSet<PhieuNhap> PhieuNhaps => Set<PhieuNhap>();
    public DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps => Set<ChiTietPhieuNhap>();
    public DbSet<CongThuc> CongThucs => Set<CongThuc>();
    public DbSet<LichSuHoatDong> LichSuHoatDongs => Set<LichSuHoatDong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoaiSanPham>(e => { e.ToTable("LoaiSanPham"); e.HasKey(x => x.MaLoai); });
        modelBuilder.Entity<SanPham>(e =>
        {
            e.ToTable("SanPham", t => t.HasTrigger("trg_SanPham_NgayCapNhat"));
            e.HasKey(x => x.MaSP);
        });
        modelBuilder.Entity<Ban>(e => { e.ToTable("Ban"); e.HasKey(x => x.MaBan); });
        modelBuilder.Entity<KhachHang>(e => { e.ToTable("KhachHang"); e.HasKey(x => x.MaKH); });
        modelBuilder.Entity<NhanVien>(e => { e.ToTable("NhanVien"); e.HasKey(x => x.MaNV); });
        modelBuilder.Entity<TaiKhoan>(e => { e.ToTable("TaiKhoan"); e.HasKey(x => x.MaTK); });
        modelBuilder.Entity<KhuyenMai>(e => { e.ToTable("KhuyenMai"); e.HasKey(x => x.MaKM); });
        modelBuilder.Entity<HoaDon>(e =>
        {
            e.ToTable("HoaDon", t => t.HasTrigger("trg_LogHuyHoaDon"));
            e.HasKey(x => x.MaHD);
        });
        modelBuilder.Entity<ChiTietHoaDon>(e =>
        {
            e.ToTable("ChiTietHoaDon", t => t.HasTrigger("trg_CapNhatTongTienHoaDon"));
            e.HasKey(x => x.MaCT);
        });
        modelBuilder.Entity<ThanhToan>(e => { e.ToTable("ThanhToan"); e.HasKey(x => x.MaTT); });
        modelBuilder.Entity<DatBan>(e => { e.ToTable("DatBan"); e.HasKey(x => x.MaDatBan); });
        modelBuilder.Entity<NhaCungCap>(e => { e.ToTable("NhaCungCap"); e.HasKey(x => x.MaNCC); });
        modelBuilder.Entity<NguyenLieu>(e => { e.ToTable("NguyenLieu"); e.HasKey(x => x.MaNL); });
        modelBuilder.Entity<PhieuNhap>(e => { e.ToTable("PhieuNhap"); e.HasKey(x => x.MaPN); });
        modelBuilder.Entity<ChiTietPhieuNhap>(e =>
        {
            e.ToTable("ChiTietPhieuNhap", t => t.HasTrigger("trg_CapNhatTonKho_Nhap"));
            e.HasKey(x => x.MaCTPN);
        });
        modelBuilder.Entity<CongThuc>(e => { e.ToTable("CongThuc"); e.HasKey(x => x.MaCongThuc); });
        modelBuilder.Entity<LichSuHoatDong>(e => { e.ToTable("LichSuHoatDong"); e.HasKey(x => x.MaLS); });

        modelBuilder.Entity<ChiTietHoaDon>()
            .Property(c => c.ThanhTien)
            .HasComputedColumnSql("CAST([SoLuong] AS DECIMAL(18,2)) * [DonGia]", stored: true);

        modelBuilder.Entity<ChiTietPhieuNhap>()
            .Property(c => c.ThanhTien)
            .HasComputedColumnSql("[SoLuong] * [DonGia]", stored: true);

        modelBuilder.Entity<CongThuc>()
            .HasIndex(c => new { c.MaSP, c.MaNL })
            .IsUnique();

        modelBuilder.Entity<SanPham>()
            .HasOne(s => s.LoaiSanPham)
            .WithMany(l => l.SanPhams)
            .HasForeignKey(s => s.MaLoai);

        modelBuilder.Entity<TaiKhoan>()
            .HasOne(t => t.KhachHang)
            .WithOne(k => k.TaiKhoan)
            .HasForeignKey<TaiKhoan>(t => t.MaKH);

        modelBuilder.Entity<TaiKhoan>()
            .HasOne(t => t.NhanVien)
            .WithMany()
            .HasForeignKey(t => t.MaNV);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.Ban)
            .WithMany()
            .HasForeignKey(h => h.MaBan);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.NhanVien)
            .WithMany()
            .HasForeignKey(h => h.MaNV);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.KhachHang)
            .WithMany()
            .HasForeignKey(h => h.MaKH);

        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.KhuyenMai)
            .WithMany()
            .HasForeignKey(h => h.MaKM);

        modelBuilder.Entity<ChiTietHoaDon>()
            .HasOne(c => c.HoaDon)
            .WithMany(h => h.ChiTietHoaDons)
            .HasForeignKey(c => c.MaHD)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietHoaDon>()
            .HasOne(c => c.SanPham)
            .WithMany()
            .HasForeignKey(c => c.MaSP);

        modelBuilder.Entity<ThanhToan>()
            .HasOne(t => t.HoaDon)
            .WithOne(h => h.ThanhToan)
            .HasForeignKey<ThanhToan>(t => t.MaHD);

        modelBuilder.Entity<DatBan>()
            .HasOne(d => d.Ban)
            .WithMany()
            .HasForeignKey(d => d.MaBan);

        modelBuilder.Entity<DatBan>()
            .HasOne(d => d.KhachHang)
            .WithMany()
            .HasForeignKey(d => d.MaKH);

        modelBuilder.Entity<NguyenLieu>()
            .HasOne(n => n.NhaCungCap)
            .WithMany(ncc => ncc.NguyenLieus)
            .HasForeignKey(n => n.MaNCC);

        modelBuilder.Entity<PhieuNhap>()
            .HasOne(p => p.NhaCungCap)
            .WithMany()
            .HasForeignKey(p => p.MaNCC);

        modelBuilder.Entity<PhieuNhap>()
            .HasOne(p => p.NhanVien)
            .WithMany()
            .HasForeignKey(p => p.MaNV);

        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasOne(c => c.PhieuNhap)
            .WithMany(p => p.ChiTietPhieuNhaps)
            .HasForeignKey(c => c.MaPN)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChiTietPhieuNhap>()
            .HasOne(c => c.NguyenLieu)
            .WithMany()
            .HasForeignKey(c => c.MaNL);

        modelBuilder.Entity<CongThuc>()
            .HasOne(c => c.SanPham)
            .WithMany()
            .HasForeignKey(c => c.MaSP);

        modelBuilder.Entity<CongThuc>()
            .HasOne(c => c.NguyenLieu)
            .WithMany()
            .HasForeignKey(c => c.MaNL);

        modelBuilder.Entity<LichSuHoatDong>()
            .HasOne(l => l.NhanVien)
            .WithMany()
            .HasForeignKey(l => l.MaNV);

        modelBuilder.Entity<DanhGiaBinhLuan>(e =>
        {
            e.ToTable("DanhGiaBinhLuan");
            e.HasKey(x => x.MaDG);
            e.Property(x => x.HoTenHienThi).HasMaxLength(100);
            e.Property(x => x.NoiDung).HasMaxLength(500);
            e.HasIndex(x => new { x.MaSP, x.MaKH })
                .IsUnique()
                .HasFilter("[MaKH] IS NOT NULL");
        });

        modelBuilder.Entity<DanhGiaBinhLuan>()
            .HasOne(d => d.SanPham)
            .WithMany()
            .HasForeignKey(d => d.MaSP)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DanhGiaBinhLuan>()
            .HasOne(d => d.KhachHang)
            .WithMany()
            .HasForeignKey(d => d.MaKH)
            .OnDelete(DeleteBehavior.SetNull);
    }
}