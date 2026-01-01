#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuickMeet.API.Filters
{
    /// <summary>
    /// Filtro de autorización para endpoints de testing.
    /// Solo permite acceso cuando AllowDangerousOperations=true en Development.
    /// En Release, esta clase no se compila gracias a #if DEBUG.
    /// </summary>
    public class RequireDangerousOperationsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>();
            var env = context.HttpContext.RequestServices
                .GetRequiredService<IWebHostEnvironment>();
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<RequireDangerousOperationsAttribute>>();

            var allowDangerous = config.GetValue<bool>("AllowDangerousOperations");
            var path = context.HttpContext.Request.Path;
            var method = context.HttpContext.Request.Method;

            if (!allowDangerous || !env.IsDevelopment())
            {
                logger.LogWarning(
                    "Blocked dangerous operation attempt. AllowDangerous={Allow}, Environment={Env}, " +
                    "Path={Path}, Method={Method}",
                    allowDangerous,
                    env.EnvironmentName,
                    path,
                    method);

                context.Result = new NotFoundResult();
                return;
            }

            logger.LogDebug(
                "Dangerous operation allowed. Path={Path}, Method={Method}",
                path,
                method);

            base.OnActionExecuting(context);
        }
    }
}
#endif
