using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace QuanLyQuanCafe.Tests.Integration;

internal sealed class AlwaysValidAntiforgery : IAntiforgery
{
    private static AntiforgeryTokenSet CreateTokens() => new("test-token", "test-cookie", "test-form", "test-field");

    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => CreateTokens();

    public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => CreateTokens();

    public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);

    public void SetCookieTokenAndHeader(HttpContext httpContext) { }

    public Task ValidateRequestAsync(HttpContext httpContext) => Task.CompletedTask;
}