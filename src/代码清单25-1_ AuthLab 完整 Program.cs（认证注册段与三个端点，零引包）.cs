// AuthLab —— cookie 认证最小闭环教学工程（net8.0，零外部包，cookie 认证随共享框架）
// 演示凭据，非真实凭据：用户名 demo_user / 口令 demo-pass-123456（仅教学演示用，生产必须从配置或环境变量注入，禁止硬编码）
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // API 形态：未登录挑战不再 302 重定向到登录页，而是直接 401
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();

var app = builder.Build();

// StatusCodePages 必须注册在认证/授权中间件之前：授权挑战在上游短路，
// 放后面 401 永远流不进它（真机实测翻车第一现场，见正文交代）
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

// 登录：核验硬编码演示凭据，通过则签发 cookie
app.MapPost("/login", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var username = form["username"].ToString();
    var password = form["password"].ToString();

    // 演示凭据，非真实凭据（仅教学用；生产从配置或环境变量注入）
    if (username == "demo_user" && password == "demo-pass-123456")
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return Results.Ok(new { message = $"logged in as {username}" });
    }
    return Results.Unauthorized();
});

// 登出
app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { message = "logged out" });
});

// 受保护端点：登录后返回 200 带用户名
app.MapGet("/me", (HttpContext ctx) =>
        Results.Ok(new { user = ctx.User.Identity?.Name }))
    .RequireAuthorization();

app.Run();