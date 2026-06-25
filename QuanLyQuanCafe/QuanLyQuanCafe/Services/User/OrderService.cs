using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services.User;

public class OrderService
{
    private readonly TooruCoffeeDbContext _db;

    public OrderService(TooruCoffeeDbContext db) => _db = db;

    public async Task<int> TaoHoaDonAsync(DatHangViewModel model, int maNv, int? maKh, List<GioHangItem> items)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("Giỏ hàng trống.");

        await using var conn = _db.Database.GetDbConnection();
        await conn.OpenAsync();

        await using var tran = await conn.BeginTransactionAsync();
        try
        {
            var maHdParam = new SqlParameter("@MaHD", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
            await using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = (SqlTransaction)tran;
                cmd.CommandText = """
                    EXEC sp_TaoHoaDon
                        @MaBan, @MaNV, @MaKH, @MaKM, @LoaiDon,
                        @TenKhach, @SDT, @DiaChiGiao, @GhiChu, @MaHD OUTPUT
                    """;
                cmd.Parameters.Add(new SqlParameter("@MaBan", (object?)model.MaBan ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@MaNV", maNv));
                cmd.Parameters.Add(new SqlParameter("@MaKH", (object?)maKh ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@MaKM", (object?)model.MaKM ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@LoaiDon", model.LoaiDon));
                cmd.Parameters.Add(new SqlParameter("@TenKhach", (object?)model.TenKhach ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@SDT", (object?)model.SDT ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DiaChiGiao", (object?)model.DiaChiGiao ?? DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@GhiChu", (object?)model.GhiChu ?? DBNull.Value));
                cmd.Parameters.Add(maHdParam);
                await cmd.ExecuteNonQueryAsync();
            }

            var maHd = (int)maHdParam.Value!;

            foreach (var item in items)
            {
                await using var cmdCt = conn.CreateCommand();
                cmdCt.Transaction = (SqlTransaction)tran;
                cmdCt.CommandText = "EXEC sp_ThemChiTietHoaDon @MaHD, @MaSP, @SoLuong, @DonGia, @GhiChu";
                cmdCt.Parameters.Add(new SqlParameter("@MaHD", maHd));
                cmdCt.Parameters.Add(new SqlParameter("@MaSP", item.MaSP));
                cmdCt.Parameters.Add(new SqlParameter("@SoLuong", item.SoLuong));
                cmdCt.Parameters.Add(new SqlParameter("@DonGia", item.DonGia));
                cmdCt.Parameters.Add(new SqlParameter("@GhiChu", (object?)item.GhiChu ?? DBNull.Value));
                await cmdCt.ExecuteNonQueryAsync();
            }

            await tran.CommitAsync();
            return maHd;
        }
        catch
        {
            await tran.RollbackAsync();
            throw;
        }
    }

    public async Task<ThanhToanViewModel?> GetHoaDonThanhToanAsync(int maHd)
    {
        var hd = await _db.HoaDons
            .Include(h => h.ChiTietHoaDons)
            .ThenInclude(c => c.SanPham)
            .FirstOrDefaultAsync(h => h.MaHD == maHd);
        if (hd == null) return null;

        return new ThanhToanViewModel
        {
            MaHD = hd.MaHD,
            TongThanhToan = hd.TongThanhToan,
            TongTienTruocGiam = hd.TongTienTruocGiam,
            GiamGia = hd.GiamGia,
            TrangThai = hd.TrangThai,
            ChiTiet = hd.ChiTietHoaDons.Select(c => new GioHangItem
            {
                MaSP = c.MaSP,
                TenSP = c.SanPham?.TenSP ?? "",
                HinhAnh = c.SanPham?.HinhAnh ?? "",
                DonGia = c.DonGia,
                SoLuong = c.SoLuong,
                GhiChu = c.GhiChu
            }).ToList()
        };
    }
}