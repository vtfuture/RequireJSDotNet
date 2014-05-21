using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Helpers
{
    using RequireJsNet.Helpers;

    internal static class PathHelpers
    {
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

        public static string ToModuleName(this string relativePath)
        {
            return relativePath.GetPathWithoutExtension().Replace(Path.DirectorySeparatorChar, '/').ToLower();
        }
        

        public static string GetRequirePath(string folder, string file)
        {
            return RequireJsNet.Helpers.PathHelpers.GetPathWithoutExtension(GetRelativePath(file, folder)).Replace("\\\\", "/").Replace("\\", "/");
        }
    }
}
