using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.RequireProcessing;
using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor
{
    /// <summary>
    /// Initializes the use of Asp.Net bundling and minification in RequireJsNet
    /// </summary>
    public class RequireWebOptimization 
    {

        public BundleCollection Bundles { get; set; }

        public string ProjectPath { get; set; }

        public string PackagePath { get; set; }

        public string[] RequireConfigs { get; set; }

        public string EntryPointOverride { get; set; }

        public bool AutoBundles { get; set; }

        public Encoding encoding;

        /// <summary>
        /// Initializes the bundling
        /// </summary>
        /// <param name="projectPath">The absolute path of the project that uses the RequireJsNet compressor</param>
        /// <param name="bundles">A list of existing Asp.Net Bundles, which will be processed later. Most likely the BundleTable.Bundles list</param>
        /// <param name="encoding">The file encoding used to encode the .js files</param>
        public RequireWebOptimization(string projectPath, BundleCollection bundles, Encoding encoding, bool autoBundles = true )
        {
            ProjectPath = projectPath;
            Bundles = bundles;
            AutoBundles = autoBundles;
            this.encoding = encoding;
        }

        public RequireWebOptimization(string projectPath, BundleCollection bundles) : this(projectPath, bundles, Encoding.UTF8){ }

        /// <summary>
        /// Creates and registers bundles by using the web optimization framework
        /// </summary>
        public void CreateAndRegisterBundles()
        {
            var files = new List<string>();
            if (RequireConfigs != null)
            {
                files = RequireConfigs.ToList();
            }

            var entryPointOveride = string.Empty;

            if (EntryPointOverride != null)
            {
                entryPointOveride = EntryPointOverride;
            }

            var configProcessor = ConfigProcessorFactory.Create(true, ProjectPath, PackagePath, entryPointOveride, files, encoding);

            // create the bundles
            var requireBundles = configProcessor.ParseConfigs();

            // add one ASP bundle for each RequireJsDotNet bundle
            foreach (var requireBundle in requireBundles)
            {
                Bundles.Add(
                    new WebOptimizationBundle(ProjectPath, requireBundle)
                    );
            }
        }
    }
}
