using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Optimization;
using RequireJsNet.Compressor.BundlePathResolver;
using RequireJsNet.Compressor.RequireProcessing;
using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor
{
	/// <summary>
	/// Initializes the use of Asp.Net bundling and minification in RequireJsNet
	/// </summary>
	public class RequireWebOptimization 
	{

		/// <summary>
		/// A list of require js config files the optimization will be based on
		/// </summary>
		public string[] RequireConfigs { get; set; }

		/// <summary>
		/// Whether to use the AutoBundler or not
		/// todo: Implement another bundler
		/// </summary>
		public bool AutoBundles { get; set; }

		/// <summary>
		/// The RequireJs entry point
		/// </summary>
		public string EntryPoint { get; set; }

		public BundlePathResolverCollection ResolverCollection { get; set; }

		/// <summary>
		/// Initializes the bundling
		/// </summary>
		/// <param name="entryPoint">the require js entry point</param>
		/// <param name="autoBundles">Whether to use auto bundles or not</param>
		public RequireWebOptimization(string entryPoint = "~/Scripts", bool autoBundles = true )
		{
			AutoBundles = autoBundles;
			EntryPoint = entryPoint;
			ResolverCollection = new BundlePathResolverCollection();
			ResolverCollection.Add(new DefaultBundlePathResolver());
		}

		/// <summary>
		/// Creates bundles by using the web optimization framework
		/// </summary>
		public BundleCollection CreateBundles()
		{
			var files = new List<string>();
			if (RequireConfigs != null)
			{
				files = RequireConfigs.ToList();
			}

			var configProcessor = new AutoBundleConfigProcessor(files, EntryPoint)
			{
				ResolverCollection = ResolverCollection
			};

			// create the bundles
			return configProcessor.ParseConfigs();
		}
	}
}
