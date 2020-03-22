using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer
{
    public static class FileUtils
    {
        public static FileSystemInfo ToFileSystemInfo(this IFileInfo file)
        {
            if (file.IsDirectory)
                return new DirectoryInfo(file.PhysicalPath);
            else
                return new FileInfo(file.PhysicalPath);
        }

        /// <summary>
        /// Returns a file/directory for the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileSystemInfo GetFileSystemInfo(string path)
        {
            if (Directory.Exists(path))
                return new DirectoryInfo(path);
            else
                return new FileInfo(path);
        }

        public static bool IsDirectory(this FileSystemInfo file)
        {
            return file is DirectoryInfo;
        }

        public static ICollection<FileSystemInfo> GetChildren(this DirectoryInfo directory)
        {
            List<FileSystemInfo> children = new List<FileSystemInfo>();
            children.AddRange(directory.GetFiles());
            children.AddRange(directory.GetDirectories());

            return children;
        }
    }
}
