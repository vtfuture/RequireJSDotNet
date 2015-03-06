// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Web.Optimization;
using RequireJsNet.Compressor.Parsing;

namespace RequireJsNet.Compressor.Transformations
{
	/// <summary>
	/// Interface for Transformations of require or define calls
	/// </summary>
	internal interface IRequireTransformation 
	{
		/// <summary>
		/// The call that will be transformed
		/// </summary>
		RequireCall RequireCall { get; set; }

		/// <summary>
		/// Executes the transformation
		/// </summary>
		/// <param name="script">the script that will be transformed and which contains the require call</param>
		void Execute(ref string script);

		/// <summary>
		/// Gets the affected range
		/// </summary>
		/// <returns>An array of integers, as indexes</returns>
		int[] GetAffectedRange();
	}
}