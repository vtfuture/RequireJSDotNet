using System.Security.Cryptography;
using RequireJsNet.Compressor.AutoDependency.Transformations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.RequireProcessing;

namespace RequireJsNet.Compressor.Models
{
	/// <summary>
	/// Class contains functionality to use Asp.Net bundling and minification 
	/// in RequireJsNet
	/// </summary>
	sealed class RequireCompressorBundle : Bundle
	{
		/// <summary>
		/// The containing config of the bundle
		/// </summary>
		public string ContainingConfig { get; set; }

		/// <summary>
		/// A Map of processed file contents, assigned by their virtual file paths
		/// </summary>
		private readonly Dictionary<string, string> contentMap; 

		/// <summary>
		/// Constructs a new WebOptimizationBundle
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <param name="builder"></param>
		public RequireCompressorBundle(string virtualPath, RequireBuilder builder)
			: base(virtualPath)
		{
			
			this.Transforms.Add(new JsMinify());
			this.Builder = builder;
			this.contentMap = new Dictionary<string, string>();
		}

		/// <summary>
		/// Includes a processed file
		/// </summary>
		/// <param name="file">The processed file</param>
		public void Include(ProcessedFile file)
		{
			this.Include(file.IncludedVirtualPath);
			this.contentMap[file.IncludedVirtualPath.ToLower()] = file.ProcessedContent;
		}

		/// <summary>
		/// Searches for any processed file content mapped to the file
		/// and if such a content is found returns it.
		/// </summary>
		/// <param name="file">a bundle file</param>
		/// <returns>The file content or the processed requie content if found</returns>
		public string RequireFileContent(BundleFile file)
		{
			var filePath = file.IncludedVirtualPath.ToLower();
			var unminifiedFilePath = filePath;

			// Web Optimization framework will choose the min.js version of a file if it exists
			// So it has to be removed  in order for the comparison to succeed 
			if (filePath.EndsWith(".min.js"))
			{
				unminifiedFilePath = filePath.Remove(filePath.Length -7, 4);
			}

			if (this.contentMap.ContainsKey(unminifiedFilePath))
			{
				return this.contentMap[unminifiedFilePath];
			}
			else if(this.contentMap.ContainsKey(filePath))
			{
				return this.contentMap[filePath]
			}
			else
			{
				return file.ApplyTransforms();
			}
		}
	}
}
