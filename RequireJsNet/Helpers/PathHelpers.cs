using System.IO;
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
    }
}
