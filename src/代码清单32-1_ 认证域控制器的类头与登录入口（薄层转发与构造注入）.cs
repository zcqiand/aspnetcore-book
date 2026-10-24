/// <summary>
/// M00.F01/F02 + M01.F04/F05 — 认证域（B1，真后端）。
/// 薄层：从 HttpContext.User 取 claims + 写 Set-Cookie 头（state CSRF）。业务在 AuthService;Controller 仅转发。
/// </summary>
[ApiController]
public sealed class AuthController(AuthService service) : AuthControllerBase
{
    private readonly AuthService _service = service;

    // @entry M01.F05.I01 — 密码登录：AuthService.Login 校验并签发会话（book anchor）
    public override Task<LoginResponse> Login([FromBody] LoginRequest body) =>
        Task.FromResult(_service.Login(body));