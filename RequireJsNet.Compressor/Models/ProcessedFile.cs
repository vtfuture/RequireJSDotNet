// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using RequireJsNet.Models;

namespace RequireJsNet.Compressor.Models
{
	/// <summary>
	/// Represents a script file that has been processed
	/// </summary>
	internal class ProcessedFile
	{
		/// <summary>
		/// The Virtual Path pointing to the original file content
		/// </summary>
		public string IncludedVirtualPath { get; set; }

		/// <summary>
		/// The processed content
		/// </summary>
		public string ProcessedContent { get; set; }

		/// <summary>
		/// A list of dependencies as require module names
		/// </summary>
		public IEnumerable<AutoBundleItem> Dependencies { get; set; } 
	}
}
