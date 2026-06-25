using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.ViewModels.User;

namespace QuanLyQuanCafe.Services.User;

public class ChatbotAdvisorService
{
    private readonly TooruCoffeeDbContext _db;

    public ChatbotAdvisorService(TooruCoffeeDbContext db) => _db = db;

    public async Task<ChatbotTuVanViewModel> LayDuLieuTuVanAsync()
    {
        var homNay = DateTime.Today;

        var loais = await _db.LoaiSanPhams
            .Where(l => l.TrangThai)
            .OrderBy(l => l.ThuTuHienThi)
            .Select(l => new { l.MaLoai, l.TenLoai, l.MoTa, l.HinhAnh })
            .ToListAsync();

        var sanPhams = await _db.SanPhams
            .Include(s => s.LoaiSanPham)
            .Where(s => s.TrangThai && s.LoaiSanPham != null && s.LoaiSanPham.TrangThai)
            .OrderBy(s => s.MaLoai)
            .ThenBy(s => s.TenSP)
            .Select(s => new
            {
                s.MaSP,
                s.TenSP,
                s.MaLoai,
                s.Gia,
                s.DonViTinh,
                s.MoTa,
                s.HinhAnh
            })
            .ToListAsync();

        var km = await _db.KhuyenMais
            .Where(k => k.TrangThai && homNay >= k.NgayBatDau && homNay <= k.NgayKetThuc)
            .OrderBy(k => k.NgayKetThuc)
            .Select(k => new { k.TenKM, k.LoaiGiam, k.GiaTriGiam, k.NgayKetThuc, k.MoTa })
            .ToListAsync();

        var loaiMon = loais.Select(l =>
        {
            var monTrongLoai = sanPhams.Where(s => s.MaLoai == l.MaLoai).ToList();
            var anhLoai = ImageHelper.ToUrl(l.HinhAnh);
            if (string.IsNullOrWhiteSpace(l.HinhAnh) && monTrongLoai.Count > 0)
                anhLoai = ImageHelper.ToUrl(monTrongLoai[0].HinhAnh);

            return new ChatbotLoaiMonViewModel
            {
                MaLoai = l.MaLoai,
                TenLoai = l.TenLoai,
                MoTaLoai = l.MoTa,
                AnhLoai = anhLoai,
                Mon = monTrongLoai.Select(s => new ChatbotMonViewModel
                {
                    MaSp = s.MaSP,
                    Ten = s.TenSP,
                    Gia = s.Gia,
                    DonVi = s.DonViTinh,
                    MoTa = string.IsNullOrWhiteSpace(s.MoTa) ? null : s.MoTa.Trim(),
                    TenLoai = l.TenLoai,
                    HinhAnh = ImageHelper.ToUrl(s.HinhAnh)
                }).ToList()
            };
        }).Where(x => x.Mon.Count > 0).ToList();

        return new ChatbotTuVanViewModel
        {
            TongSoMon = sanPhams.Count,
            AnhQuan = ImageHelper.ToUrl("images/Ca_phe_latte_art.jpg"),
            LoaiMon = loaiMon,
            KhuyenMai = km.Select(k => new ChatbotKhuyenMaiViewModel
            {
                Ten = k.TenKM,
                MoTaGiam = k.LoaiGiam == "PhanTram"
                    ? $"Giảm {k.GiaTriGiam:0.#}%"
                    : $"Giảm {k.GiaTriGiam:N0} đ",
                HetHan = k.NgayKetThuc.ToString("dd/MM/yyyy")
            }).ToList()
        };
    }
}