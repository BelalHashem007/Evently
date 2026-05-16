namespace EventBookingSystem.Middlewares
{
    public class RedirectMiddleware 
    {
        private readonly RequestDelegate _next;

        public RedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                IList<string> prohibitedPaths = ["/auth/login", "/auth/signup"];
                if(prohibitedPaths.Contains(httpContext.Request.Path.Value, StringComparer.OrdinalIgnoreCase))
                {
                    httpContext.Response.Redirect("/");
                    return;
                }
            }

            await _next(httpContext);
        }
    }

    public static class RedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseRedirectMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RedirectMiddleware>();
        }
    }
}
