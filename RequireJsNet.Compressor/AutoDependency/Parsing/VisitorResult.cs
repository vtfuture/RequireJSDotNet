// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Collections.Generic;
using System.Linq;

namespace RequireJsNet.Compressor.Parsing
{
	/// <summary>
	/// Represents the Result of a RequireVisitor visit
	/// </summary>
	internal class VisitorResult
	{
		/// <summary>
		/// Constructs a new Result
		/// </summary>
		public VisitorResult()
		{
			RequireCalls = new List<RequireCall>();    
		}

		/// <summary>
		/// A list of found require calls
		/// </summary>
		public List<RequireCall> RequireCalls { get; set; }

		/// <summary>
		/// flattens the result to a list of calls
		/// </summary>
		/// <returns>The call list</returns>
		public List<RequireCall> GetFlattened()
		{
			if (RequireCalls == null)
			{
				return null;
			}

			var result = new List<RequireCall>();

			var currentList = RequireCalls;
			currentList.ForEach(x => { result.Add(x); });   

			while (currentList != null && currentList.Any())
			{
				currentList = currentList.SelectMany(r => r.Children).ToList();
				currentList.ForEach(x => { result.Add(x); });    
			}

			return result;
		}
	}
}
