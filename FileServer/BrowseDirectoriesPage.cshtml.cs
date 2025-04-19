using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;

namespace FileServer
{
    public class BrowseDirectoriesPageModel : PageModel
    {
        protected static readonly Regex PATTERN_GET_FILE_PATH = new Regex("([^/]+)$");

        /// <summary>
        /// Input: the directory contents that we want to display.
        /// </summary>
        public IEnumerable<IFileInfo> Directory
        {
            get;
            set;
        }

        /// <summary>
        /// The current file path relative to the server root.
        /// </summary>
        public string DirectoryPath
        {
            get
            {
                return Request.Path.Value;
            }
        }

        /// <summary>
        /// The file server's name (i.e. host name).
        /// </summary>
        public string HostName
        {
            get
            {
                string host = Request.Host.Value;
                //remove port
                host = new Regex(@":\d+$").Replace(host, "");

                return host;
            }
        }

        /// <summary>
        /// True if the parent directory's path is still part of the server's file structure (i.e. can be navigated to).
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ParentDirectory);
            }
        }

        /// <summary>
        /// The parent directory's path if it is still part of the server's file structure (i.e. can be navigated to).
        /// Navigate to the parent by setting this as href for a hyperlink.
        /// </summary>
        /// <seealso cref="CanGoBack"/>
        public string ParentDirectory
        {
            get
            {
                string currentPath = DirectoryPath;
                if(!(string.IsNullOrWhiteSpace(currentPath) || currentPath.Equals("/")))
                {
                    if(currentPath.EndsWith("/"))
                    {
                        return "../";
                    }
                    else
                    {
                        return "./";
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Returns a path that links to the given file when set as href in a hyperlink.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetPath(IFileInfo file)
        {
            string filePath;
            string currentPath;

            var matchPath = PATTERN_GET_FILE_PATH.Match(DirectoryPath);
            if(matchPath.Success)
            {
                currentPath = matchPath.Groups[1].Value;
                filePath = currentPath + "/" + file.Name;
            }
            else
            {
                currentPath = "";
                filePath = file.Name;
            }

            return filePath;
        }

        public void OnGet()
        {
        }
    }
}
