using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using QuanLyQuanCafe.Data;
using Xunit;

namespace QuanLyQuanCafe.Tests.Integration;

public class ThucDonIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ThucDonIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    [Fact]
    public async Task ThucDon_Index_ShouldReturnSuccessAndContainMenuContent()
    {
        var response = await _client.GetAsync("/ThucDon");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Thực đơn", html);
        Assert.Contains("Tìm món", html);
    }

    [Fact]
    public async Task ThucDon_Index_WithSearchQuery_ShouldFilterResults()
    {
        var response = await _client.GetAsync("/ThucDon?q=cafe");

        var html = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("cafe", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ThucDon_ChiTiet_ExistingProduct_ShouldReturnOk()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TooruCoffeeDbContext>();
        var sp = db.SanPhams.First(s => s.TenSP == "Cà phê sữa đá");

        var response = await _client.GetAsync($"/ThucDon/ChiTiet/{sp.MaSP}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await IntegrationTestHelpers.ReadHtmlAsync(response);
        Assert.Contains(sp.TenSP, html);
    }
}
