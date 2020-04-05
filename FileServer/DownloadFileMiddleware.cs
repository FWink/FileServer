using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace FileServer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class DownloadFileMiddleware : FileMiddleware
    {
        private readonly RequestDelegate _next;

        public DownloadFileMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(IsFileRequest(httpContext.Request))
            {
                await HandleFileDownload(httpContext);
            }
            else
            {
                await _next(httpContext);
            }
        }

        /// <summary>
        /// Sends the requested file (<see cref="GetFile(HttpRequest)"/>) to the client via <see cref="HttpContext.Response"/>.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected internal async Task HandleFileDownload(HttpContext httpContext)
        {
            var file = GetFile(httpContext.Request);

            SetResponseFileName(httpContext.Response, file.Name);
            httpContext.Response.ContentLength = file.Length;
            await httpContext.Response.SendFileAsync(file);
        }

        /// <summary>
        /// Tests whether the given request is a request to download a file.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal bool IsFileRequest(HttpRequest request)
        {
            return request.Method == "GET" & request.Query.Count == 0 && GetFile(request).Exists;
        }

        /// <summary>
        /// Fetches the request's referenced file if applicable.
        /// See <see cref="IFileInfo.Exists"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal IFileInfo GetFile(HttpRequest request)
        {
            string path = GetRequestPath(request);

            if (_hostingEnvironment.WebRootFileProvider == null)
                return new NotFoundFileInfo(path);

            var file = _hostingEnvironment.WebRootFileProvider.GetFileInfo(path);
            return file;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class DownloadFileMiddlewareExtensions
    {
        public static IApplicationBuilder UseDownloadFileMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DownloadFileMiddleware>();
        }
    }
}
