using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services.User;

public class PaymentService
{
    private readonly TooruCoffeeDbContext _db;

    public PaymentService(TooruCoffeeDbContext db) => _db = db;

    public async Task<(bool Success, string? Error)> ThanhToanAsync(ThanhToanViewModel model, int? maNv)
    {
        try
        {
            await using var conn = _db.Database.GetDbConnection();
            await conn.OpenAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                EXEC sp_ThanhToanHoaDon
                    @MaHD, @PhuongThuc, @SoTien, @MaGiaoDich, @MaNVThanhToan
                """;
            cmd.Parameters.Add(new SqlParameter("@MaHD", model.MaHD));
            cmd.Parameters.Add(new SqlParameter("@PhuongThuc", model.PhuongThuc));
            cmd.Parameters.Add(new SqlParameter("@SoTien", model.TongThanhToan));
            cmd.Parameters.Add(new SqlParameter("@MaGiaoDich", (object?)model.MaGiaoDich ?? DBNull.Value));
            cmd.Parameters.Add(new SqlParameter("@MaNVThanhToan", (object?)maNv ?? DBNull.Value));
            await cmd.ExecuteNonQueryAsync();
            return (true, null);
        }
        catch (SqlException ex)
        {
            return (false, ex.Message);
        }
    }

    public static readonly string[] PhuongThucThanhToan =
    [
        "Tiền mặt", "Thẻ", "Chuyển khoản", "Momo", "ZaloPay", "ShopeePay", "Khác"
    ];
}