using System.Security.Claims;
using KcetasAboneApi.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace KcetasAboneApi.Middlewares;

public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdStr = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out var userId))
            {
                dbContext.MevcutKullaniciId = userId;
            }
        }

        await _next(context);
    }
}
