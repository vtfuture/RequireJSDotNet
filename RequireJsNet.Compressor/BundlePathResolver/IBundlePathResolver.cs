using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.Helper;

namespace RequireJsNet.Compressor.BundlePathResolver
{
	/// <summary>
	/// Interface for resolving bundle paths 
	/// </summary>
	public interface IBundlePathResolver
	{
		/// <summary>
		/// Returns a bundle path for the corresponding bundle
		/// that will be used for bundle overrides in the RequireJs config
		/// </summary>
		/// <param name="pathResolver">A PathResolver instance</param>
		/// <param name="context">The bundle context used for the require bundles</param>
		/// <param name="bundle">The bundle for which the path has to be returned</param>
		/// <returns>The bundle path in RequireJs compatible format</returns>
		string Resolve(PathResolver pathResolver, BundleContext context, Bundle bundle);
	}
}
