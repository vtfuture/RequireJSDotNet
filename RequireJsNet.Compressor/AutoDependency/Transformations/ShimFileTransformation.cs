// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Linq;

using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Compressor.Transformations;
using RequireJsNet.Helpers;

namespace RequireJsNet.Compressor.AutoDependency.Transformations
{
	/// <summary>
	/// Wrapps a define module around a shim file
	/// </summary>
	internal class ShimFileTransformation : IRequireTransformation
	{
		/// <summary>
		/// The shim dependencies
		/// </summary>
		private readonly List<string> dependencies;

		/// <summary>
		/// The shim's module name
		/// </summary>
		private readonly string moduleName;

		/// <summary>
		/// The shim export variable
		/// </summary>
		private readonly string exports;

		/// <summary>
		/// Creates the transformation
		/// </summary>
		/// <param name="moduleName">the name of the shim module</param>
		/// <param name="dependencies">a list of dependencies</param>
		/// <param name="export">The export statement</param>
		public ShimFileTransformation(string moduleName, List<string> dependencies, string export)
		{
			this.moduleName = moduleName;
			this.dependencies = dependencies;
			this.exports = export;
		}

		/// <inheritdoc />
		public RequireCall RequireCall { get; set; }

		/// <summary>
		/// Creates the transformation
		/// </summary>
		/// <param name="moduleName">the name of the shim module</param>
		/// <param name="dependencies">a list of dependencies</param>
		/// <param name="export">The export statement</param>
		/// <returns>the created transformation</returns>
		public static ShimFileTransformation Create(string moduleName, List<string> dependencies, string export)
		{
			return new ShimFileTransformation(moduleName, dependencies, export);
		}

		/// <inheritdoc />
		public void Execute(ref string script)
		{
			var depString = string.Format("[{0}]", string.Join(",", dependencies.Select(r => "'" + r + "'")));
			var exportStatement = string.Empty;

			if (exports != null)
			{
				exportStatement = string.Format("window.{0} = {0}; return {0};", exports);
			}

			script = string.Format("define('{0}', {1}, function () {{{2}{3}{2}{4}{2}}});", moduleName, depString, Environment.NewLine, script, exportStatement);
		}

		/// <inheritdoc />
		public int[] GetAffectedRange()
		{
			// this range is only here to position this as the first transformation that should be executed
			// it's likely that no other transformations will be run on this script since it doesn't have a req call,
			// so it doesn't really matter anyway
			return new[] { 0, 1 };
		}
	}
}
