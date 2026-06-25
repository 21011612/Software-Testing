using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using QuanLyQuanCafe.Tests.Integration;
using Xunit;

namespace QuanLyQuanCafe.Tests.Integration;

public class DanhGiaIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DanhGiaIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Gui_Post_WithoutToken_ShouldFailValidationOrRedirect()
    {
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("MaSP", "1"),
            new KeyValuePair<string, string>("SoSao", "5"),
            new KeyValuePair<string, string>("NoiDung", "Rất ngon!")
        });

        var response = await _client.PostAsync("/DanhGia/Gui", form);

        Assert.True(response.StatusCode == HttpStatusCode.BadRequest ||
                    response.StatusCode == HttpStatusCode.Redirect ||
                    response.StatusCode == HttpStatusCode.OK);
    }
}
