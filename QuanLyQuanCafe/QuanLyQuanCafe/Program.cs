using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafe.Data;
using QuanLyQuanCafe.Helpers;
using QuanLyQuanCafe.Services;
using QuanLyQuanCafe.Services.Admin;
using QuanLyQuanCafe.Services.Atm;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TooruCoffeeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TooruCoffee")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.AccessDeniedPath = "/QuanTri/DangNhap";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RegistrationService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<DatabaseSeedService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<ProductImageUploadService>();
builder.Services.AddScoped<ChatbotAdvisorService>();
builder.Services.AddScoped<DanhGiaService>();
builder.Services.AddScoped<AdminOrderService>();
builder.Services.AddSingleton<BankServerService>();
builder.Services.AddScoped<AtmMachineService>();
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationExpanders.Add(new AdminUserViewLocationExpander());
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeedService>();
    await seeder.EnsureTaiKhoanPasswordsAsync();
    await seeder.EnsureDanhGiaBinhLuanTableAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Make the generated Program class public so that WebApplicationFactory<Program> 
// can be used from the test project (required for Integration Tests).
public partial class Program { }