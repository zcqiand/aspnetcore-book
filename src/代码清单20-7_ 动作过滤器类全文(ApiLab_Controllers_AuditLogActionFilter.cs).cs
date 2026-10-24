using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiLab.Controllers;

// ── MVC 过滤器家族代表(第20章): 与 ch19 的端点过滤器 AuditTrailFilter 两家族对照 ──
// 挂接点不同: 端点过滤器要显式挂到端点/组上, 动作过滤器随控制器管线自动生效;
// OnActionExecuted 时状态码已写上响应, 不必再借 Response.OnStarting 绕时机坑。
public class AuditLogActionFilter(ILogger<AuditLogActionFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        logger.LogInformation("动作过滤器进入: {Method} {Path}",
            context.HttpContext.Request.Method, context.HttpContext.Request.Path);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        logger.LogInformation("动作过滤器离开: {Method} {Path} -> {StatusCode}",
            context.HttpContext.Request.Method, context.HttpContext.Request.Path,
            context.HttpContext.Response.StatusCode);
    }
}