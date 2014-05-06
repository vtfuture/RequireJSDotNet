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
    }
}
