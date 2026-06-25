using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace QuanLyQuanCafe.Tests.Integration;

public class DonHangIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public DonHangIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GioHang_Get_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/DonHang/GioHang");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CapNhatGioHang_Post_ShouldRedirectToGioHang()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("maSp", "1"),
            new KeyValuePair<string, string>("soLuong", "2")
        });

        var response = await _client.PostAsync("/DonHang/CapNhatGioHang", form);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/DonHang/GioHang", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task DatHang_Get_EmptyCart_ShouldRedirectToThucDon()
    {
        using var client = await IntegrationTestHelpers.CreateAuthenticatedClientAsync(_factory);
        var response = await client.GetAsync("/DonHang/DatHang");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/ThucDon", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task LichSu_Get_Unauthorized_ShouldRedirectToLogin()
    {
        var response = await _client.GetAsync("/DonHang/LichSu");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/TaiKhoan/DangNhap", response.Headers.Location?.ToString());
    }
}