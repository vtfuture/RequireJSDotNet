using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;
using RequireJsNet.Compressor.Helper;

namespace RequireJsNet.Compressor.BundlePathResolver
{
	/// <summary>
	/// Collection of BundlePathResolver objects
	/// </summary>
	public class BundlePathResolverCollection
	{
		/// <summary>
		/// internal resolver list
		/// </summary>
		private readonly List<IBundlePathResolver> resolvers = new List<IBundlePathResolver>();

		/// <summary>
		/// Clears the collection
		/// </summary>
		public void Clear()
		{
			lock (resolvers)
			{
				resolvers.Clear();
			}

		}

		/// <summary>
		/// Prepends an item to the collection
		/// </summary>
		/// <param name="resolver">the resolver to be prepended</param>
		public void Prepend(IBundlePathResolver resolver)
		{
			lock (resolvers)
			{
				resolvers.Insert(0, resolver);
			}
		}

		/// <summary>
		/// Appends an item to the collection
		/// </summary>
		/// <param name="resolver">the resolver to be added</param>
		public void Add(IBundlePathResolver resolver)
		{
			lock (resolvers)
			{
				resolvers.Add(resolver);
			}
		}

		/// <summary>
		/// Resolves the path for a bundle by iterating over the collected resolvers
		/// and using the first return value as the resolved path
		/// </summary>
		/// <param name="pathResolver">A PathResolver instance</param>
		/// <param name="context">The bundle context used for the require bundles</param>
		/// <param name="bundle">The bundle for which the path has to be returned</param>
		/// <returns>The bundle path in RequireJs compatible format</returns>
		internal string Resolve(PathResolver pathResolver, BundleContext context, Bundle bundle)
		{
			string result = null;

			lock (resolvers)
			{
				foreach (var resolver in resolvers)
				{
					result = resolver.Resolve(pathResolver, context, bundle);
					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}
	}
}



