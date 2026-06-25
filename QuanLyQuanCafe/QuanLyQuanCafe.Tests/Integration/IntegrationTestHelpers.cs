using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace QuanLyQuanCafe.Tests.Integration;

public static class IntegrationTestHelpers
{
    public static async Task<string> ReadHtmlAsync(HttpResponseMessage response) =>
        WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

    public static async Task<HttpClient> CreateAuthenticatedClientAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "TenDangNhap", "testuser" },
            { "MatKhau", "Test@123" }
        });

        var response = await client.PostAsync("/TaiKhoan/DangNhap", form);
        if (response.StatusCode != HttpStatusCode.Redirect)
            throw new InvalidOperationException($"Login failed with status {response.StatusCode}");

        return client;
    }
}