using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Services.Admin;

public class StatisticsService
{
    private static readonly string[] TrangThaiDaThu = ["Đã thanh toán", "Hoàn thành"];
    private readonly TooruCoffeeDbContext _db;

    public StatisticsService(TooruCoffeeDbContext db) => _db = db;

    public async Task<ThongKeViewModel> LayThongKeAsync(DateTime tuNgay, DateTime denNgay)
    {
        tuNgay = tuNgay.Date;
        denNgay = denNgay.Date;
        if (tuNgay > denNgay)
            (tuNgay, denNgay) = (denNgay, tuNgay);

        var vm = new ThongKeViewModel { TuNgay = tuNgay, DenNgay = denNgay };
        await DocStoredProcAsync(vm, tuNgay, denNgay);
        await NapBieuDoBoSungAsync(vm, tuNgay, denNgay);
        return vm;
    }

    public async Task<AdminDashboardViewModel> LayDashboardAsync(DateTime? tuNgay = null, DateTime? denNgay = null)
    {
        var den = denNgay?.Date ?? DateTime.Today;
        var tu = tuNgay?.Date ?? den.AddDays(-30);
        if (tu > den)
            tu = den.AddDays(-30);

        var homNay = DateTime.Today;
        var paid = TrangThaiDaThu;

        return new AdminDashboardViewModel
        {
            TongSanPham = await _db.SanPhams.CountAsync(s => s.TrangThai),
            BanDangPhucVu = await _db.Bans.CountAsync(b => b.TrangThai == "Đang phục vụ"),
            HoaDonChoThanhToan = await _db.HoaDons.CountAsync(h => h.TrangThai == "Chờ thanh toán"),
            KhachHang = await _db.KhachHangs.CountAsync(),
            DoanhThuHomNay = await _db.HoaDons
                .Where(h => h.NgayLap.Date == homNay && paid.Contains(h.TrangThai))
                .SumAsync(h => h.TongThanhToan),
            DonHomNay = await _db.HoaDons
                .CountAsync(h => h.NgayLap.Date == homNay && paid.Contains(h.TrangThai)),
            DatBanSapToi = await _db.DatBans.CountAsync(d =>
                d.NgayDat >= homNay && d.TrangThai == "Đã xác nhận"),
            ThongKe = await LayThongKeAsync(tu, den)
        };
    }

    private async Task DocStoredProcAsync(ThongKeViewModel vm, DateTime tuNgay, DateTime denNgay)
    {
        // Không dispose connection của DbContext — chỉ mở/đóng tạm thời.
        var conn = _db.Database.GetDbConnection();
        var openedHere = false;
        try
        {
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
                openedHere = true;
            }

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_ThongKeDoanhThu @TuNgay, @DenNgay";
            cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay));
            cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay));

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vm.TheoNgay.Add(new DoanhThuTheoNgayRow
                {
                    Ngay = reader.GetDateTime(0),
                    SoDon = reader.GetInt32(1),
                    DoanhThuGoc = reader.GetDecimal(2),
                    TongGiam = reader.GetDecimal(3),
                    DoanhThuThuc = reader.GetDecimal(4),
                    TrungBinhDon = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5)
                });
            }

            if (await reader.NextResultAsync() && await reader.ReadAsync())
            {
                vm.TongKet = new ThongKeTongKetRow
                {
                    TongSoDon = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    TongDoanhThuGoc = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                    TongGiamGia = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2),
                    TongDoanhThuThuc = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3)
                };
            }
        }
        finally
        {
            if (openedHere && conn.State == ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    private async Task NapBieuDoBoSungAsync(ThongKeViewModel vm, DateTime tuNgay, DateTime denNgay)
    {
        var denCuoi = denNgay.AddDays(1);
        var paid = TrangThaiDaThu;

        vm.TopSanPham = await (
            from ct in _db.ChiTietHoaDons
            join hd in _db.HoaDons on ct.MaHD equals hd.MaHD
            join sp in _db.SanPhams on ct.MaSP equals sp.MaSP
            where hd.NgayLap >= tuNgay && hd.NgayLap < denCuoi && paid.Contains(hd.TrangThai)
            group ct by sp.TenSP into g
            orderby g.Sum(x => x.SoLuong * x.DonGia) descending
            select new TopSanPhamRow
            {
                TenSP = g.Key,
                SoLuong = g.Sum(x => x.SoLuong),
                DoanhThu = g.Sum(x => x.SoLuong * x.DonGia)
            })
            .Take(8)
            .ToListAsync();

        vm.TheoLoaiDon = await _db.HoaDons
            .Where(h => h.NgayLap >= tuNgay && h.NgayLap < denCuoi && paid.Contains(h.TrangThai))
            .GroupBy(h => h.LoaiDon)
            .Select(g => new NhanLabelRow
            {
                Nhan = g.Key,
                SoLuong = g.Count(),
                GiaTri = g.Sum(x => x.TongThanhToan)
            })
            .ToListAsync();

        vm.TheoPhuongThuc = await (
            from t in _db.ThanhToans
            join hd in _db.HoaDons on t.MaHD equals hd.MaHD
            where hd.NgayLap >= tuNgay && hd.NgayLap < denCuoi && paid.Contains(hd.TrangThai)
            group t by t.PhuongThuc into g
            select new NhanLabelRow
            {
                Nhan = g.Key,
                SoLuong = g.Count(),
                GiaTri = g.Sum(x => x.SoTien)
            }).ToListAsync();

        vm.TheoLoaiSanPham = await (
            from ct in _db.ChiTietHoaDons
            join hd in _db.HoaDons on ct.MaHD equals hd.MaHD
            join sp in _db.SanPhams on ct.MaSP equals sp.MaSP
            join loai in _db.LoaiSanPhams on sp.MaLoai equals loai.MaLoai
            where hd.NgayLap >= tuNgay && hd.NgayLap < denCuoi && paid.Contains(hd.TrangThai)
            group ct by loai.TenLoai into g
            orderby g.Sum(x => x.SoLuong * x.DonGia) descending
            select new NhanLabelRow
            {
                Nhan = g.Key,
                SoLuong = g.Sum(x => x.SoLuong),
                GiaTri = g.Sum(x => x.SoLuong * x.DonGia)
            })
            .ToListAsync();
    }
}