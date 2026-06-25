using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services.User;

public class DanhGiaService
{
    private readonly TooruCoffeeDbContext _db;

    public DanhGiaService(TooruCoffeeDbContext db) => _db = db;

    public async Task<SanPhamDanhGiaTongHop?> LayTongHopAsync(int maSp)
    {
        var dict = await LayTongHopNhieuAsync([maSp]);
        return dict.GetValueOrDefault(maSp);
    }

    public async Task<Dictionary<int, SanPhamDanhGiaTongHop>> LayTongHopNhieuAsync(IEnumerable<int> maSps)
    {
        var ids = maSps.Distinct().ToList();
        if (ids.Count == 0) return [];

        var rows = await _db.DanhGiaBinhLuans
            .Where(d => ids.Contains(d.MaSP) && d.TrangThai)
            .GroupBy(d => d.MaSP)
            .Select(g => new SanPhamDanhGiaTongHop
            {
                MaSP = g.Key,
                DiemTrungBinh = g.Average(x => x.SoSao),
                SoDanhGia = g.Count()
            })
            .ToListAsync();

        return rows.ToDictionary(x => x.MaSP);
    }

    public async Task<List<DanhGiaHienThiViewModel>> LayDanhSachHienThiAsync(int maSp, int? maKh = null, int take = 30)
    {
        return await _db.DanhGiaBinhLuans
            .AsNoTracking()
            .Where(d => d.MaSP == maSp && d.TrangThai)
            .OrderByDescending(d => d.NgayTao)
            .Take(take)
            .Select(d => new DanhGiaHienThiViewModel
            {
                MaDG = d.MaDG,
                HoTenHienThi = d.HoTenHienThi,
                SoSao = d.SoSao,
                NoiDung = d.NoiDung,
                NgayTao = d.NgayTao,
                LaCuaToi = maKh.HasValue && d.MaKH == maKh
            })
            .ToListAsync();
    }

    public async Task<DanhGiaBinhLuan?> LayDanhGiaCuaKhachAsync(int maSp, int maKh) =>
        await _db.DanhGiaBinhLuans.FirstOrDefaultAsync(d => d.MaSP == maSp && d.MaKH == maKh);

    public async Task<(bool Success, string? Error)> LuuDanhGiaAsync(
        DanhGiaFormViewModel model,
        int? maKh,
        string? hoTenTuTaiKhoan)
    {
        if (!await _db.SanPhams.AnyAsync(s => s.MaSP == model.MaSP && s.TrangThai))
            return (false, "Sản phẩm không tồn tại.");

        var hoTen = (hoTenTuTaiKhoan ?? model.HoTenHienThi ?? "").Trim();
        if (string.IsNullOrEmpty(hoTen))
            return (false, "Vui lòng nhập họ tên hoặc đăng nhập để bình luận.");

        if (model.SoSao < 1 || model.SoSao > 5)
            return (false, "Số sao từ 1 đến 5.");

        var noiDung = (model.NoiDung ?? "").Trim();
        if (noiDung.Length < 5)
            return (false, "Bình luận tối thiểu 5 ký tự.");

        DanhGiaBinhLuan entity;
        if (maKh.HasValue)
        {
            var existing = await LayDanhGiaCuaKhachAsync(model.MaSP, maKh.Value);
            if (existing != null)
            {
                existing.SoSao = model.SoSao;
                existing.NoiDung = noiDung;
                existing.HoTenHienThi = hoTen;
                existing.NgayTao = DateTime.Now;
                existing.TrangThai = true;
                await _db.SaveChangesAsync();
                return (true, null);
            }

            entity = new DanhGiaBinhLuan
            {
                MaSP = model.MaSP,
                MaKH = maKh,
                HoTenHienThi = hoTen,
                SoSao = model.SoSao,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                TrangThai = true
            };
        }
        else
        {
            entity = new DanhGiaBinhLuan
            {
                MaSP = model.MaSP,
                MaKH = null,
                HoTenHienThi = hoTen,
                SoSao = model.SoSao,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                TrangThai = true
            };
        }

        _db.DanhGiaBinhLuans.Add(entity);
        await _db.SaveChangesAsync();
        return (true, null);
    }
}