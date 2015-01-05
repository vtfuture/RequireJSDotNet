// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.IO;
using System.Linq;
using System.Web;

namespace RequireJsNet.Helpers
{
    internal static class PathHelpers
    {
        public static string MapPath(this HttpContextBase context, string path)
        {
            if (path.StartsWith("~"))
            {
                path = context.Server.MapPath(path);
            }

            return path;
        }

        public static void VerifyFileExists(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Could not find config file", path);
            }
        }

        public static string GetPathWithoutExtension(this string path)
        {
            if (path != null)
            {
                int i;
                if ((i = path.LastIndexOf('.')) == -1)
                {
                    return path; // No path extension found
                }
                else
                {
                    return path.Substring(0, i);
                }
            }

            return null;
        }

        public static string GetOverridePath(string originalPath)
        {
            var beforeExtension = originalPath.LastIndexOf(".");
            if (beforeExtension == -1)
            {
                return null;
            }

            var newName = originalPath.Substring(0, beforeExtension) 
                            + ".override"
                            + originalPath.Substring(beforeExtension, originalPath.Length - beforeExtension);
            return newName;
        }

        public static string GetRequirePath(this string path)
        {
            return path.GetPathWithoutExtension().Replace("\\\\", "/").Replace("\\", "/");
        }

        public static string ToModuleName(this string relativePath)
        {
            var initial = relativePath;
            relativePath = initial.Replace('/', Path.DirectorySeparatorChar);
            var directory = Path.GetDirectoryName(relativePath);
            var file = relativePath.EndsWith(".js") ? Path.GetFileNameWithoutExtension(relativePath)
                                                    : Path.GetFileName(relativePath);
            var name = file;
            if (!string.IsNullOrEmpty(directory))
            {
                name = directory.ToLower() + Path.DirectorySeparatorChar + name;
            }
            name = name.Replace(Path.DirectorySeparatorChar, '/');
            return name;
        }

        public static string GetRelativePath(string filespec, string folder)
        {
            var pathUri = new Uri(filespec);

            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }


        public static string GetRequireRelativePath(string folder, string file)
        {
            return GetRelativePath(file, folder).ToModuleName();
        }

        // will return the exact fileName for a supplied file path
        // not this only returns for the fileName, the directory part of the path will be the same as the one supplied
        public static string GetExactFilePath(string suppliedName)
        {
           return Directory.GetFiles(
                   Path.GetDirectoryName(suppliedName), 
                   Path.GetFileName(suppliedName))
               .FirstOrDefault();
        }
    }
}
