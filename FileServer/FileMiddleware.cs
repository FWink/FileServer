using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FileServer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public abstract class FileMiddleware
    {
        protected readonly IWebHostEnvironment _hostingEnvironment;

        protected FileMiddleware(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Returns a sanitized/normalized request string that can be used to query <see cref="IFileProvider"/> for example.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal string GetRequestPath(HttpRequest request)
        {
            return new Regex(@"^[\\/.]+").Replace(request.Path.Value, "");
        }

        /// <summary>
        /// Sanitizes the given query file name to prevent it from referencing parent files.
        /// </summary>
        /// <param name="fromQuery"></param>
        /// <returns></returns>
        protected internal string SanitizeFilePath(string fromQuery)
        {
            return new Regex(@"\.\.[/\\]").Replace(fromQuery, "");
        }

        /// <summary>
        /// Sets the required HTTP header so that the given response is downloaded as file with the given name from a browser.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="filename"></param>
        protected internal void SetResponseFileName(HttpResponse response, string filename)
        {
            response.Headers.Add("Content-Disposition", "attachment; filename=" + Uri.EscapeDataString(filename));
        }
    }
}
