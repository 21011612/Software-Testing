using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCafe.Services;

namespace QuanLyQuanCafe.ViewComponents;

public class ChatbotTuVanViewComponent : ViewComponent
{
    private readonly ChatbotAdvisorService _advisor;

    public ChatbotTuVanViewComponent(ChatbotAdvisorService advisor) => _advisor = advisor;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var data = await _advisor.LayDuLieuTuVanAsync();
        return View(data);
    }
}