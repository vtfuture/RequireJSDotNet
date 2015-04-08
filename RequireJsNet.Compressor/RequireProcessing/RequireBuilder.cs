using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.Helper;
using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor.RequireProcessing
{
	/// <summary>
	/// Builds require bundles
	/// </summary>
	internal class RequireBuilder : IBundleBuilder
	{
		/// <summary>
		/// A map of all bundle files
		/// </summary>
		protected Dictionary<string, string> BundleFileMap { get; set; } 

		/// <summary>
		/// Constructor creates a new builder instance
		/// </summary>
		public RequireBuilder()
		{
	
			BundleFileMap = new Dictionary<string, string>();
		}

		/// <summary>
		/// Builds the bundle content
		/// </summary>
		/// <param name="bundle">the bundle to build</param>
		/// <param name="context">the bundle context</param>
		/// <param name="files">a list of files composing the bundle</param>
		/// <returns></returns>
		public string BuildBundleContent(Bundle bundle, BundleContext context, IEnumerable<BundleFile> files)
		{
			// Make sure that bundle is a RequireCompressorBundle
			if (!(bundle is RequireCompressorBundle))
			{
				throw new ArgumentException("RequireBuilder expects object of type RequireCompressorBundle","bundle");
			}

			var requireBundle = (RequireCompressorBundle) bundle;

			var responseBuilder = new StringBuilder();

			// Remove duplicates and iterate over files
			foreach (var file in files.Distinct(new BundleEqualityComparer()))
			{
				
				// get file content
				var content = requireBundle.RequireFileContent(file);

				// prepend processed content to response
				responseBuilder.Append(content);
			}

			return responseBuilder.ToString();
		}
	}
}
