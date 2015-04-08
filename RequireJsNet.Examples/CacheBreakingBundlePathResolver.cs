using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Optimization;
using RequireJsNet.Compressor.BundlePathResolver;
using RequireJsNet.Compressor.Helper;
using System.Text;

namespace RequireJsNet.Examples
{
	public class CacheBreakingBundlePathResolver : IBundlePathResolver
	{
		/// <inheritdoc />
		public string Resolve(PathResolver pathResolver, BundleContext context, Bundle bundle)
		{
			if (bundle == null)
			{
				throw new ArgumentNullException("bundle");
			}

			if (context == null)
			{
				throw new ArgumentNullException("context");
			}

			var content = bundle.GenerateBundleResponse(context).Content;

			var hashAlgorithm = new SHA256Managed();

			var queryString = "?v=" + HttpServerUtility.UrlTokenEncode(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(content)));

			return VirtualPathUtility.ToAbsolute(bundle.Path) + queryString;
		}
	}
}