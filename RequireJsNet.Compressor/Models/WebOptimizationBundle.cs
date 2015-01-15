using RequireJsNet.Compressor.AutoDependency.Transformations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Optimization;

namespace RequireJsNet.Compressor.Models
{
    /// <summary>
    /// Class contains functionality to use Asp.Net bundling and minification 
    /// in RequireJsNet
    /// </summary>
    class WebOptimizationBundle : System.Web.Optimization.Bundle
    {
        /// <summary>
        /// Constructs a new WebOptimizationBundle
        /// </summary>
        /// <param name="projectPath">The absolute path of the project that uses the RequireJsNet compressor</param>
        /// <param name="requireBundle">The RequireJsNet bundle</param>
        public WebOptimizationBundle(string projectPath, Bundle requireBundle)
            : base(VirtualPathFromRequireBundle(projectPath, requireBundle))
        {
            this.Transforms.Add(new ConvertRequireBundleToWebOptimizationBundle(requireBundle));
            this.Transforms.Add(new JsMinify());
        }

        /// <summary>
        /// Retrieves the virtaul path for a WebOptimizationBundle from an existing
        /// RequireJsNet Bundle
        /// </summary>
        /// <param name="projectPath">The absolute path of the project that uses the RequireJsNet compressor</param>
        /// <param name="bundle">The RequireJsNet bundle</param>
        /// <returns></returns>
        protected static string VirtualPathFromRequireBundle(string projectPath, Bundle bundle)
        {
            //  convert absolute path to relative one            
            return MakeRelativePath(projectPath, bundle.Output);
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        protected static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.ToUpperInvariant() == "FILE")
            {
                relativePath = relativePath.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);
            }

            return "~/" + relativePath.Replace("\\", "/");
        }
    }
}
