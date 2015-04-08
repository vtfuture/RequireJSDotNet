using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.Helper;

namespace RequireJsNet.Compressor
{
	/// <summary>
	/// Implements a cache busting mechanism for bundles based on their content
	/// </summary>
	static class BundleCacheBuster
	{
		/// <summary>
		/// Returns a bundle path with a cache busting
		/// </summary>
		/// <param name="resolver"></param>
		/// <param name="context"></param>
		/// <param name="bundle"></param>
		/// <returns></returns>
		public static string GetBundlePath(PathResolver resolver, BundleContext context, Bundle bundle)
		{
			var md5 = Md5Hash(bundle.GenerateBundleResponse(context).Content);

			bundle.Path = bundle.Path + "_" + md5;
		}

		/// <summary>
		/// Calculates a MD5 hash for a string
		/// </summary>
		/// <param name="input">The input string</param>
		/// <returns>The hash in hex format</returns>
		private static string Md5Hash(string input)
		{
			// step 1, calculate MD5 hash from input
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}
	}
}
