using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Services.Admin;

public class AdminOrderService
{
    private readonly TooruCoffeeDbContext _db;

    public AdminOrderService(TooruCoffeeDbContext db) => _db = db;

    public async Task<HoaDon?> LayChiTietAsync(int maHd) =>
        await _db.HoaDons
            .Include(h => h.Ban)
            .Include(h => h.NhanVien)
            .Include(h => h.KhachHang)
            .Include(h => h.KhuyenMai)
            .Include(h => h.ChiTietHoaDons).ThenInclude(c => c.SanPham)
            .Include(h => h.ThanhToan)
            .FirstOrDefaultAsync(h => h.MaHD == maHd);

    public async Task<(bool Success, int? MaHD, string? Error)> TaoHoaDonAsync(HoaDonTaoAdminViewModel model, int maNv)
    {
        try
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            var maHdParam = new SqlParameter("@MaHD", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                EXEC sp_TaoHoaDon
                    @MaBan, @MaNV, @MaKH, @LoaiDon,
                    @TenKhach, @SDT, @DiaChiGiao, @GhiChu, @MaHD OUTPUT
                """;
            cmd.Parameters.Add(new SqlParameter("@MaBan", (object?)model.MaBan ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@MaNV", maNv));
            cmd.Parameters.Add(new SqlParameter("@MaKH", (object?)model.MaKH ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@LoaiDon", model.LoaiDon));
            cmd.Parameters.Add(new SqlParameter("@TenKhach", (object?)model.TenKhach ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@SDT", (object?)model.SDT ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@DiaChiGiao", (object?)model.DiaChiGiao ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@GhiChu", (object?)model.GhiChu ?? DBNull.Value));
            cmd.Parameters.Add(maHdParam);
            await cmd.ExecuteNonQueryAsync();
            return (true, (int)maHdParam.Value!, null);
        }
        catch (SqlException ex)
        {
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ThemChiTietAsync(int maHd, HoaDonChiTietLineViewModel line)
    {
        var sp = await _db.SanPhams.FindAsync(line.MaSP);
        if (sp == null) return (false, "Sản phẩm không tồn tại.");

        var hd = await _db.HoaDons.FindAsync(maHd);
        if (hd == null) return (false, "Hóa đơn không tồn tại.");
        if (hd.TrangThai != "Chờ thanh toán")
            return (false, "Chỉ thêm món vào hóa đơn đang chờ thanh toán.");

        try
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_ThemChiTietHoaDon @MaHD, @MaSP, @SoLuong, @DonGia, @GhiChu";
            cmd.Parameters.Add(new SqlParameter("@MaHD", maHd));
            cmd.Parameters.Add(new SqlParameter("@MaSP", line.MaSP));
            cmd.Parameters.Add(new SqlParameter("@SoLuong", line.SoLuong));
            cmd.Parameters.Add(new SqlParameter("@DonGia", sp.Gia));
            cmd.Parameters.Add(new SqlParameter("@GhiChu", (object?)line.GhiChu ?? DBNull.Value));
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> XoaChiTietAsync(int maHd, int maCt)
    {
        var ct = await _db.ChiTietHoaDons.FirstOrDefaultAsync(c => c.MaCT == maCt && c.MaHD == maHd);
        if (ct == null) return (false, "Dòng chi tiết không tồn tại.");

        var hd = await _db.HoaDons.FindAsync(maHd);
        if (hd == null) return (false, "Hóa đơn không tồn tại.");
        if (hd.TrangThai != "Chờ thanh toán")
            return (false, "Không xóa được dòng trên hóa đơn đã thanh toán/hủy.");

        _db.ChiTietHoaDons.Remove(ct);
        await _db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ApDungKhuyenMaiAsync(int maHd, int maKm)
    {
        try
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_ApDungKhuyenMai @MaHD, @MaKM";
            cmd.Parameters.Add(new SqlParameter("@MaHD", maHd));
            cmd.Parameters.Add(new SqlParameter("@MaKM", maKm));
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> HuyHoaDonAsync(int maHd, int? maNv)
    {
        try
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "EXEC sp_HuyHoaDon @MaHD, @MaNV";
            cmd.Parameters.Add(new SqlParameter("@MaHD", maHd));
            cmd.Parameters.Add(new SqlParameter("@MaNV", (object?)maNv ?? DBNull.Value));
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
    }
}