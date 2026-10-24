public class AuthController : AuthControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtIssuer _jwt;
    private readonly SaasSessionStore _sessions;
    private readonly FailedLoginStore _failedLogins;
    private readonly IAuditWriter _audit;

    public AuthController(
        AppDbContext db,
        JwtIssuer jwt,
        SaasSessionStore sessions,
        FailedLoginStore failedLogins,
        IAuditWriter audit)
    {
        _db = db;
        _jwt = jwt;
        _sessions = sessions;
        _failedLogins = failedLogins;
        _audit = audit;
    }