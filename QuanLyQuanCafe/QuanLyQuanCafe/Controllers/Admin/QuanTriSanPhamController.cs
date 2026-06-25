using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.ViewModels;
using QuanLyQuanCafe.ViewModels.Admin;

namespace QuanLyQuanCafe.Controllers.Admin;

[Authorize(Roles = "Admin,NhanVien")]
public class QuanTriSanPhamController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly ProductImageUploadService _upload;

    public QuanTriSanPhamController(TooruCoffeeDbContext db, ProductImageUploadService upload)
    {
        _db = db;
        _upload = upload;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _db.SanPhams
            .Include(s => s.LoaiSanPham)
            .OrderBy(s => s.MaLoai)
            .ThenBy(s => s.TenSP)
            .ToListAsync();
        return View(list);
    }

    public async Task<IActionResult> Them()
    {
        await LoadLoaiAsync();
        return View(new SanPhamAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Them(SanPhamAdminViewModel model)
    {
        if (!await TryResolveHinhAnhAsync(model, null))
        {
            await LoadLoaiAsync();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            await LoadLoaiAsync();
            return View(model);
        }

        _db.SanPhams.Add(new SanPham
        {
            TenSP = model.TenSP.Trim(),
            MaLoai = model.MaLoai,
            Gia = model.Gia,
            MoTa = model.MoTa,
            HinhAnh = model.HinhAnh!,
            DonViTinh = model.DonViTinh,
            KichCo = model.KichCo,
            TrangThai = model.TrangThai,
            NgayTao = DateTime.Now,
            NgayCapNhat = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm sản phẩm mới.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Sua(int id)
    {
        var sp = await _db.SanPhams.FindAsync(id);
        if (sp == null) return NotFound();
        await LoadLoaiAsync();
        return View(MapToVm(sp));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sua(int id, SanPhamAdminViewModel model)
    {
        if (id != model.MaSP) return BadRequest();

        var sp = await _db.SanPhams.FindAsync(id);
        if (sp == null) return NotFound();

        if (!await TryResolveHinhAnhAsync(model, sp.HinhAnh))
        {
            await LoadLoaiAsync();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            await LoadLoaiAsync();
            return View(model);
        }

        sp.TenSP = model.TenSP.Trim();
        sp.MaLoai = model.MaLoai;
        sp.Gia = model.Gia;
        sp.MoTa = model.MoTa;
        sp.HinhAnh = model.HinhAnh!;
        sp.DonViTinh = model.DonViTinh;
        sp.KichCo = model.KichCo;
        sp.TrangThai = model.TrangThai;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật sản phẩm.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Xoa(int id)
    {
        var sp = await _db.SanPhams.FindAsync(id);
        if (sp == null) return NotFound();

        var coDon = await _db.ChiTietHoaDons.AnyAsync(c => c.MaSP == id);
        if (coDon)
        {
            sp.TrangThai = false;
            TempData["Success"] = "Sản phẩm đã có trong hóa đơn — đã ẩn khỏi thực đơn.";
        }
        else
        {
            _db.SanPhams.Remove(sp);
            TempData["Success"] = "Đã xóa sản phẩm.";
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> TryResolveHinhAnhAsync(SanPhamAdminViewModel model, string? currentPath)
    {
        if (model.AnhUpload is { Length: > 0 })
        {
            var (ok, path, err) = await _upload.SaveProductImageAsync(model.AnhUpload, model.TenSP);
            if (!ok)
            {
                ModelState.AddModelError(nameof(model.AnhUpload), err ?? "Không lưu được ảnh.");
                return false;
            }
            model.HinhAnh = path;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(model.HinhAnh))
        {
            model.HinhAnh = NormalizeImagePath(model.HinhAnh);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            model.HinhAnh = currentPath;
            return true;
        }

        ModelState.AddModelError(nameof(model.AnhUpload), "Vui lòng tải ảnh lên hoặc nhập đường dẫn ảnh.");
        return false;
    }

    private static string NormalizeImagePath(string path)
    {
        path = path.Trim().Replace('\\', '/');
        if (!path.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            path = "images/" + path.TrimStart('/');
        return path;
    }

    private async Task LoadLoaiAsync()
    {
        ViewBag.Loais = new SelectList(
            await _db.LoaiSanPhams.Where(l => l.TrangThai).OrderBy(l => l.ThuTuHienThi).ToListAsync(),
            "MaLoai", "TenLoai");
    }

    private static SanPhamAdminViewModel MapToVm(SanPham sp) => new()
    {
        MaSP = sp.MaSP,
        TenSP = sp.TenSP,
        MaLoai = sp.MaLoai,
        Gia = sp.Gia,
        MoTa = sp.MoTa,
        HinhAnh = sp.HinhAnh,
        DonViTinh = sp.DonViTinh,
        KichCo = sp.KichCo,
        TrangThai = sp.TrangThai
    };
}