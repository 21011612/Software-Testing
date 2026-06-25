using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Models;
using QuanLyQuanCafe.Services;

namespace QuanLyQuanCafe.Controllers.User;

public class HomeController : Controller
{
    private readonly TooruCoffeeDbContext _db;
    private readonly CartService _cart;

    public HomeController(TooruCoffeeDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public async Task<IActionResult> Index()
    {
        var homNay = DateTime.Today;
        ViewBag.MonNoiBat = await _db.SanPhams
            .Include(s => s.LoaiSanPham)
            .Where(s => s.TrangThai)
            .OrderBy(s => s.MaSP)
            .Take(8)
            .ToListAsync();
        ViewBag.Loais = await _db.LoaiSanPhams
            .Where(l => l.TrangThai)
            .OrderBy(l => l.ThuTuHienThi)
            .ToListAsync();
        ViewBag.KhuyenMai = await _db.KhuyenMais
            .Where(k => k.TrangThai && homNay >= k.NgayBatDau && homNay <= k.NgayKetThuc)
            .OrderBy(k => k.NgayKetThuc)
            .Take(3)
            .ToListAsync();
        ViewBag.SoMonGioHang = _cart.SoMon();
        return View();
    }

    public IActionResult GioiThieu() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}