using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FileServer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class FileNotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        public FileNotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 404;
            return Task.FromResult(false);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class FileNotFoundMiddlewareExtensions
    {
        public static IApplicationBuilder UseFileNotFoundMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FileNotFoundMiddleware>();
        }
    }
}
