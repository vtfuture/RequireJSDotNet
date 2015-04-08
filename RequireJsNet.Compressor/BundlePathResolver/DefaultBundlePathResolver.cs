using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.Helper;

namespace RequireJsNet.Compressor.BundlePathResolver
{
	/// <summary>
	/// Default bundle path behaviour
	/// </summary>
	class DefaultBundlePathResolver : IBundlePathResolver
	{
		/// <inheritdoc />
		public string Resolve(PathResolver pathResolver, BundleContext context, Bundle bundle)
		{
			return pathResolver.VirtualPathToRequirePath(bundle.Path);
		}
	}
}
