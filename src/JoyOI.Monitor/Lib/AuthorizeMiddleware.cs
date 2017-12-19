using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace JoyOI.Monitor.Lib
{
    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.Value.ToString().ToLower() != "/account/login" && httpContext.Session.GetInt32("login") != 1)
            {
                httpContext.Response.Redirect("/account/login");
            }
            else
            {
                await _next(httpContext);
            }
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthorizeMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthorize(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthorizeMiddleware>();
        }
    }
}
