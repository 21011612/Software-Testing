using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using Xunit;

namespace QuanLyQuanCafe.Tests.Integration;

public class TaiKhoanIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public TaiKhoanIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task DangKy_Post_ValidData_ShouldRedirectToDangNhap_And_CreateUserInDb()
    {
        var formData = new Dictionary<string, string>
        {
            { "HoTen", "Nguyen Van Test" },
            { "SDT", "0987654321" },
            { "Email", "test.integration@example.com" },
            { "TenDangNhap", "testuser_integration" },
            { "MatKhau", "Password123!" },
            { "XacNhanMatKhau", "Password123!" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await _client.PostAsync("/TaiKhoan/DangKy", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? "";
        Assert.Contains("/TaiKhoan/DangNhap", location);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TooruCoffeeDbContext>();

        var khachHang = await db.KhachHangs.FirstOrDefaultAsync(k => k.SDT == "0987654321");
        var taiKhoan = await db.TaiKhoans.FirstOrDefaultAsync(t => t.TenDangNhap == "testuser_integration");

        Assert.NotNull(khachHang);
        Assert.Equal("Nguyen Van Test", khachHang.HoTen);
        Assert.NotNull(taiKhoan);
        Assert.Equal("KhachHang", taiKhoan.VaiTro);
    }

    [Fact]
    public async Task DangKy_Post_DuplicateSDT_ShouldReturnView_WithError()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TooruCoffeeDbContext>();
            db.KhachHangs.Add(new QuanLyQuanCafe.Models.KhachHang
            {
                HoTen = "Existing",
                SDT = "0911222333",
                NgayTao = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        var formData = new Dictionary<string, string>
        {
            { "HoTen", "New User" },
            { "SDT", "0911222333" },
            { "TenDangNhap", "newuser123" },
            { "MatKhau", "Password123!" },
            { "XacNhanMatKhau", "Password123!" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await _client.PostAsync("/TaiKhoan/DangKy", content);
        var html = await IntegrationTestHelpers.ReadHtmlAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Số điện thoại đã được đăng ký", html);
    }

    [Fact]
    public async Task DangNhap_Post_ValidCredentials_ShouldRedirectToThucDon()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var regService = scope.ServiceProvider.GetRequiredService<QuanLyQuanCafe.Services.RegistrationService>();
            await regService.DangKyAsync(new QuanLyQuanCafe.ViewModels.User.DangKyViewModel
            {
                HoTen = "Login Test User",
                SDT = "0977888999",
                TenDangNhap = "logintestuser",
                MatKhau = "SecurePass123!",
                XacNhanMatKhau = "SecurePass123!"
            });
        }

        var formData = new Dictionary<string, string>
        {
            { "TenDangNhap", "logintestuser" },
            { "MatKhau", "SecurePass123!" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await _client.PostAsync("/TaiKhoan/DangNhap", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString();
        Assert.True((location?.Contains("/ThucDon") ?? false) || (location?.Contains("Index") ?? false), 
            $"Expected redirect to ThucDon, but got: {location}");
    }

    [Fact]
    public async Task DangNhap_Post_WrongPassword_ShouldReturnView_WithError()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var regService = scope.ServiceProvider.GetRequiredService<QuanLyQuanCafe.Services.RegistrationService>();
            await regService.DangKyAsync(new QuanLyQuanCafe.ViewModels.User.DangKyViewModel
            {
                HoTen = "Wrong Pass User",
                SDT = "0966555444",
                TenDangNhap = "wrongpassuser",
                MatKhau = "CorrectPass123",
                XacNhanMatKhau = "CorrectPass123"
            });
        }

        var formData = new Dictionary<string, string>
        {
            { "TenDangNhap", "wrongpassuser" },
            { "MatKhau", "SaiMatKhau" }
        };

        var content = new FormUrlEncodedContent(formData);

        var response = await _client.PostAsync("/TaiKhoan/DangNhap", content);
        var html = await IntegrationTestHelpers.ReadHtmlAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("không đúng", html, StringComparison.OrdinalIgnoreCase);
    }
}
