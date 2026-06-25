using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCafe.Services.Atm;
using QuanLyQuanCafe.ViewModels.Atm;

namespace QuanLyQuanCafe.Controllers.Atm;

public class AtmController : Controller
{
    private readonly AtmMachineService _atm;

    public AtmController(AtmMachineService atm) => _atm = atm;

    public IActionResult Index()
    {
        _atm.XoaPhien();
        return View(new NhapTheViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NhapThe(NhapTheViewModel model)
    {
        if (!ModelState.IsValid)
            return View("Index", model);

        var (ok, err) = _atm.NhapThe(model.SoThe);
        if (!ok)
        {
            ModelState.AddModelError(nameof(model.SoThe), err!);
            return View("Index", model);
        }

        return RedirectToAction(nameof(NhapPin));
    }

    public IActionResult NhapPin()
    {
        var soThe = HttpContext.Session.GetString(AtmSessionKeys.SoThe);
        if (string.IsNullOrEmpty(soThe))
            return RedirectToAction(nameof(Index));

        return View(new NhapPinViewModel { SoThe = soThe });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult NhapPin(NhapPinViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (ok, err) = _atm.XacNhanPin(model.MaPin);
        if (!ok)
        {
            ModelState.AddModelError(nameof(model.MaPin), err!);
            return View(model);
        }

        return RedirectToAction(nameof(Menu));
    }

    public IActionResult Menu()
    {
        var vm = _atm.LayMenu();
        if (vm == null)
            return RedirectToAction(nameof(Index));
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Menu(string luaChon)
    {
        if (!_atm.DaDangNhap())
            return RedirectToAction(nameof(Index));

        return luaChon switch
        {
            LuaChonAtm.KiemTraSoDu => View("KetQua", _atm.KiemTraSoDu()),
            LuaChonAtm.RutTien => RedirectToAction(nameof(RutTien)),
            LuaChonAtm.Huy => View("KetQua", _atm.HuyGiaoDich()),
            _ => RedirectToAction(nameof(Menu)),
        };
    }

    public IActionResult RutTien()
    {
        if (!_atm.DaDangNhap())
            return RedirectToAction(nameof(Index));

        var soThe = HttpContext.Session.GetString(AtmSessionKeys.SoThe) ?? "";
        return View(new RutTienViewModel { SoThe = soThe });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RutTien(RutTienViewModel model)
    {
        if (!_atm.DaDangNhap())
            return RedirectToAction(nameof(Index));

        if (!ModelState.IsValid)
            return View(model);

        return View("KetQua", _atm.RutTien(model.SoTien));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult KetThuc()
    {
        return View("KetQua", _atm.KetThucPhien());
    }
}