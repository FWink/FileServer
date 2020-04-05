using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace FileServer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BrowseDirectoriesMiddleware : FileMiddleware
    {
        private readonly RequestDelegate _next;

        public BrowseDirectoriesMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(IsDirectoryRequest(httpContext.Request))
            {
                await HandleDirectoyBrowsing(httpContext);
            }
            else
            {
                await _next(httpContext);
            }
        }

        /// <summary>
        /// Handles the directory browsing by redirecting to the proper handler/writing the
        /// directory content to the HttpResponse.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected internal async Task HandleDirectoyBrowsing(HttpContext httpContext)
        {
            //build and display the "BrowseDirectoriesPage"
            var model = new BrowseDirectoriesPageModel();
            model.Directory = GetDirectory(httpContext.Request);

            var modelState = model.ModelState;

            var metaDataProvider = httpContext.RequestServices.GetService(typeof(IModelMetadataProvider)) as IModelMetadataProvider;
            var viewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<BrowseDirectoriesPageModel>(metaDataProvider, modelState);

            viewData.Model = model;

            var context = new PageContext(new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor(),
                modelState));
            model.PageContext = context;

            var result = new ViewResult
            {
                ViewName = "BrowseDirectoriesPage.cshtml",
                ViewData = viewData
            };

            await result.ExecuteResultAsync(context);
        }

        /// <summary>
        /// Tests whether the given request is a request to browse a directory's content.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal bool IsDirectoryRequest(HttpRequest request)
        {
            return request.Method == "GET" & request.Query.Count == 0 && GetDirectory(request).Exists;
        }

        /// <summary>
        /// Fetches the request's referenced directory with its content if applicable.
        /// See <see cref="IDirectoryContents.Exists"/>
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal IDirectoryContents GetDirectory(HttpRequest request)
        {
            if (_hostingEnvironment.WebRootFileProvider == null)
                return new NotFoundDirectoryContents();

            string path = GetRequestPath(request);

            var content = _hostingEnvironment.WebRootFileProvider.GetDirectoryContents(path);
            return content;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class BrowseDirectoriesMiddlewareExtensions
    {
        public static IApplicationBuilder UseBrowseDirectoriesMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BrowseDirectoriesMiddleware>();
        }
    }
}
