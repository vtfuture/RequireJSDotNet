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
using RequireJsNet.Models;

namespace RequireJsNet.Compressor
{
	/// <summary>
	/// Base class for Config Processing
	/// </summary>
	internal abstract class ConfigProcessor
	{
		/// <summary>
		/// The config file name pattern
		/// </summary>
		protected const string ConfigFileName = "RequireJS.*";

		/// <summary>
		/// A Bundle Context for the bundles
		/// </summary>
		protected BundleContext Context;

		/// <summary>
		/// The RequireJs entry point as virtual path
		/// </summary>
		protected string EntryPoint { get; set; }

		/// <summary>
		/// The RequireJs configuration
		/// </summary>
		protected ConfigurationCollection Configuration { get; set; }

		/// <summary>
		/// The paths to the RequireJs configuration files
		/// </summary>
		protected List<string> FilePaths { get; set; }

		/// <summary>
		/// The function used to generate bundles out of 
		/// the configuration
		/// </summary>
		/// <returns>The generated Bundles</returns>
		public abstract BundleCollection ParseConfigs();
		
		/// <summary>
		/// Finds configuration files and adds them
		/// </summary>
		protected void FindConfigs()
		{
			if (FilePaths.Any())
			{
				return;
			}
			
			var files = Directory.GetFiles(HttpContext.Current.Server.MapPath("/"), ConfigFileName);
			foreach (var file in files.Where(r => !r.ToLower().Contains(".override.")))
			{
				FilePaths.Add(file);
			}

			if (!FilePaths.Any())
			{
				throw new ArgumentException("No Require config files were provided and none were found in the project directory.");
			}
		}
	}
}
