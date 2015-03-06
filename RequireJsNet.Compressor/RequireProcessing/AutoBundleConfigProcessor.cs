// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using RequireJsNet.Compressor.AutoDependency;
using RequireJsNet.Compressor.BundlePathResolver;
using RequireJsNet.Compressor.Helper;
using RequireJsNet.Compressor.Models;
using RequireJsNet.Configuration;
using RequireJsNet.Models;

namespace RequireJsNet.Compressor.RequireProcessing
{
	/// <summary>
	/// Processes the RequireJs config and generates the AutoBundles
	/// </summary>
	internal class AutoBundleConfigProcessor : ConfigProcessor
	{
		/// <summary>
		/// The PathResolver instance
		/// </summary>
		private readonly PathResolver resolver;

		public BundlePathResolverCollection ResolverCollection { get; set; }

		/// <summary>
		/// Creates a new AutoBundleConfigProcessor
		/// </summary>
		/// <param name="filePaths">The paths to the RequireJs conif files</param>
		/// <param name="entryPoint"></param>
		public AutoBundleConfigProcessor(List<string> filePaths, string entryPoint)
		{
			FilePaths = filePaths;

			EntryPoint = entryPoint;
			resolver = new PathResolver(EntryPoint);
		}

		/// <summary>
		/// Parses the configuration and returns the generated bundles
		/// </summary>
		/// <returns>A BundleCollection of RequireCompressorBundles</returns>
		public override BundleCollection ParseConfigs()
		{
			var bundles = new BundleCollection();

			FindConfigs();

			var loader = new ConfigLoader(
				FilePaths,
				new ExceptionThrowingLogger(),
				new ConfigLoaderOptions { ProcessBundles = true });

			// Load Configurations
			Configuration = loader.Get();

			// new builder instance includes the set configuration
			var builder = new RequireBuilder();

			var alreadyProcessedFileNames = new List<string>();
			
			// iterate over all bundle definitions of the configuration and build the equivalent web optimization bundles
			foreach (var bundle in Configuration.AutoBundles.Bundles)
			{
				var bundleResult = new RequireCompressorBundle(resolver.GetOutputPath(bundle.OutputPath, bundle.Id), builder)
									   {
											ContainingConfig = bundle.ContainingConfig,
									   };

				// add bundle to collection
				bundles.Add(bundleResult);

				// construct queue utilities
				var includeQueue = new Queue<AutoBundleItem>();

				// initially enqueuing
				EnqueueBundles(bundle.Includes, includeQueue, alreadyProcessedFileNames);

				// construct dependency resolver
				var processor = new ScriptProcessor(Configuration, resolver);

				// iterate over the includes
				while(includeQueue.Any())
				{
					var include = includeQueue.Dequeue();

					if (!string.IsNullOrEmpty(include.File))
					{
						// include file
						var filePath = resolver.RequirePathToVirtualPath(include.File);
		
						var processedFile = processor.Process(filePath);
						bundleResult.Include(processedFile);
						alreadyProcessedFileNames.Add(include.File);

						EnqueueBundles(processedFile.Dependencies, includeQueue, alreadyProcessedFileNames);
					}
					else if (!string.IsNullOrEmpty(include.Directory))
					{

						// get virtual directory path
						var virtualDirectoryPath = resolver.RequirePathToVirtualPath(include.Directory, false);
						// get absolute directory path
						var absoluteDirectoryPath = HttpContext.Current.Server.MapPath(virtualDirectoryPath);

						// get all js files in directory and subdirectories
						var files = Directory.EnumerateFiles(absoluteDirectoryPath, "*.js", SearchOption.AllDirectories);

						// convert to virtual paths
						var fileArray = files.Select(r => virtualDirectoryPath + r.Replace(absoluteDirectoryPath, "").Replace("\\", "/")).ToArray();

						// process files
						EnqueueBundles(fileArray.Select(r => new AutoBundleItem { File = resolver.VirtualPathToRequirePath(r) }), includeQueue, alreadyProcessedFileNames);
					}
				}
			}

			Context = new BundleContext(new HttpContextWrapper(HttpContext.Current),bundles, EntryPoint);

			this.WriteOverrideConfigs(bundles);

			return bundles;
		}


		/// <summary>
		/// Pushes a set of bundles into a queue if the do not already exists in the queue or
		/// in an exclude set
		/// </summary>
		/// <param name="bundles">The set of bundles</param>
		/// <param name="queue">The existing queue</param>
		/// <param name="excludeItems">The set of file names to be excluded from enqueuing</param>
		private static void EnqueueBundles(IEnumerable<AutoBundleItem> bundles, Queue<AutoBundleItem> queue, IEnumerable<string> excludeItems)
		{
			foreach (var bundle in bundles.Distinct(new BundleEqualityComparer()))
			{
				if (!excludeItems.Any(r => string.Equals(r, bundle.File, StringComparison.CurrentCultureIgnoreCase))
					&& !queue.Any(r => string.Equals(r.File, bundle.File, StringComparison.CurrentCultureIgnoreCase)))
				{
					queue.Enqueue(bundle);
				}
			}
		}

		/// <summary>
		/// Writes an override for the configuration
		/// </summary>
		/// <param name="bundles">The generated bundles</param>
		private void WriteOverrideConfigs(IEnumerable<Bundle> bundles)
		{
			var groupings = bundles.GroupBy(r => ((RequireCompressorBundle)r).ContainingConfig).ToList();
			foreach (var grouping in groupings)
			{
				var path = RequireJsNet.Helpers.PathHelpers.GetOverridePath(grouping.Key);
				var writer = WriterFactory.CreateWriter(path, null);
				var collection = this.ComposeCollection(grouping.ToList());
				writer.WriteConfig(collection);    
			}
		}

		/// <summary>
		/// Adds the bundle overrides to the paths section of the RequireJs config
		/// and composes the override collection
		/// </summary>
		/// <param name="bundles">The generated bundles</param>
		/// <returns></returns>
		private ConfigurationCollection ComposeCollection(IEnumerable<Bundle> bundles)
		{
			var conf = new ConfigurationCollection();
			conf.Overrides = new List<CollectionOverride>();
			foreach (var bundle in bundles)
			{
				var scripts = bundle.EnumerateFiles(Context).Select(r => resolver.VirtualPathToRequirePath(r.IncludedVirtualPath)).ToList();
				var bundlePath = ResolverCollection.Resolve(resolver, Context, bundle);

				var paths = new RequirePaths
								{
									PathList = new List<RequirePath>()
								};
				foreach (var script in scripts)
				{
					paths.PathList.Add(new RequirePath
										   {
											   Key = script,
											   Value = bundlePath
										   });
				}

				var over = new CollectionOverride
							   {
								   BundleId = bundle.Path,
								   BundledScripts = scripts,
								   Paths = paths
							   };
				conf.Overrides.Add(over);
			}

			return conf;
		}
	}
}
