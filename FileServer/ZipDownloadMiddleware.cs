using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace FileServer
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ZipDownloadMiddleware : FileMiddleware
    {
        private readonly RequestDelegate _next;

        public ZipDownloadMiddleware(RequestDelegate next, IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(IsZipRequest(httpContext.Request))
            {
                await HandleZip(httpContext);
            }
            else
            {
                await _next(httpContext);
            }
        }

        /// <summary>
        /// Writes the requested files as zip into the response stream.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected internal async Task HandleZip(HttpContext httpContext)
        {
            var rootFile = GetRequestFile(httpContext.Request);
            var files = GetFiles(httpContext.Request);

            if(files.Count == 1 && files.First().IsDirectory())
            {
                //exactly one file is selected which is a directory => use that as root instead and select its children
                var rootDirectory = files.First() as DirectoryInfo;

                rootFile = rootDirectory;
                files = rootDirectory.GetChildren();
            }

            string root = rootFile.FullName;
            if(!(rootFile is DirectoryInfo))
            {
                root = (rootFile as FileInfo).Directory.FullName;
            }

            SetResponseFileName(httpContext.Response, rootFile.Name + ".zip");
            httpContext.Response.Headers.Add("Content-Type", "application/zip");
            using (var zipArchive = new ZipArchive(httpContext.Response.Body, ZipArchiveMode.Create))
            {
                await WriteZippedFiles(zipArchive, root, files, httpContext.RequestAborted);
            }
        }

        /// <summary>
        /// Recursively writes the given files (including plain files and directories with all their descendants) into the given ZIP archive.
        /// No compression is applied.
        /// </summary>
        /// <param name="zipArchive"></param>
        /// <param name="relativeToPath"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task WriteZippedFiles(ZipArchive zipArchive, string relativeToPath, ICollection<FileSystemInfo> files, CancellationToken cancellationToken)
        {
            foreach (var fileDirectoy in files)
            {
                if (fileDirectoy is DirectoryInfo)
                {
                    var directory = fileDirectoy as DirectoryInfo;
                    //write the entire directory recursively
                    await WriteZippedFiles(zipArchive, relativeToPath, directory.GetChildren(), cancellationToken);
                }
                else
                {
                    var file = fileDirectoy as FileInfo;
                    var fileEntry = zipArchive.CreateEntry(GetRelativePath(file, relativeToPath), CompressionLevel.NoCompression);

                    using (var fileInput = file.OpenRead())
                    using (var fileOutput = fileEntry.Open())
                    {
                        await fileInput.CopyToAsync(fileOutput, cancellationToken);
                    }
                }
            }
        }

        protected internal string GetRelativePath(FileSystemInfo file, string rootPath)
        {
            return new Regex("^" + Regex.Escape(rootPath) + @"[\\/]*").Replace(file.FullName, "");
        }

        /// <summary>
        /// Appends the given file's <see cref="FileSystemInfo.Name"/> to the given path.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        protected internal string AppendPath(FileSystemInfo file, string rootPath)
        {
            if(!Regex.IsMatch(rootPath, @"[\\/]*$"))
            {
                string separator = "\\";

                var match = Regex.Match(rootPath, @"[\\/]");
                if(match.Success)
                {
                    separator = match.Groups[1].Value;
                }

                rootPath += separator;
            }

            return rootPath + file.Name;
        }

        /// <summary>
        /// Tests whether the given request is a request to download a batch of files as a zip archive.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal bool IsZipRequest(HttpRequest request)
        {
            return request.Method == "GET" & request.Query.ContainsKey("zip") && GetFiles(request).Count > 0;
        }

        /// <summary>
        /// Fetches the request's referenced files/directories that should be returned as .zip archive.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal ICollection<FileSystemInfo> GetFiles(HttpRequest request)
        {
            ICollection<FileSystemInfo> files = new List<FileSystemInfo>();

            if (_hostingEnvironment.WebRootFileProvider == null)
                return files;

            var fileRoot = GetRequestFile(request);

            var queryFiles = request.Query["zip"];
            if(queryFiles.Count == 0 || (queryFiles.Count == 1 && string.IsNullOrWhiteSpace(queryFiles.First())))
            {
                //entire directory (or a single file I guess)
                files.Add(fileRoot);
            }
            else
            {
                if(fileRoot is DirectoryInfo)
                {
                    var directoy = fileRoot as DirectoryInfo;

                    foreach(var query in queryFiles)
                    {
                        string queryPath = fileRoot.FullName + "/" + SanitizeFilePath(query);

                        FileSystemInfo file = FileUtils.GetFileSystemInfo(queryPath);
                        files.Add(file);
                    }
                }
            }

            //filter files that don't exist
            return files.Where(file => file != null && file.Exists).ToHashSet();
        }

        /// <summary>
        /// Returns the file or directoy referenced by the given request without considering query parameters.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected internal FileSystemInfo GetRequestFile(HttpRequest request)
        {
            string path = GetRequestPath(request);
            if(_hostingEnvironment.WebRootPath != null)
            {
                path = _hostingEnvironment.WebRootPath + "/" + path;
            }

            return FileUtils.GetFileSystemInfo(path);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ZipDownloadMiddlewareExtensions
    {
        public static IApplicationBuilder UseZipDownloadMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ZipDownloadMiddleware>();
        }
    }
}
