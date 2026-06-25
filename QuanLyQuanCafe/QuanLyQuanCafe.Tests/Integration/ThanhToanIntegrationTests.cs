using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using QuanLyQuanCafe.Tests.Integration;
using Xunit;

namespace QuanLyQuanCafe.Tests.Integration;

public class ThanhToanIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ThanhToanIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task ChiTiet_Get_Unauthorized_ShouldRedirectToLogin()
    {
        var response = await _client.GetAsync("/ThanhToan/ChiTiet/1");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task XacNhan_Post_Unauthorized_ShouldRedirectToLogin()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("MaHD", "1"),
            new KeyValuePair<string, string>("PhuongThuc", "Tiền mặt")
        });

        var response = await _client.PostAsync("/ThanhToan/XacNhan", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location?.ToString());
    }
}
