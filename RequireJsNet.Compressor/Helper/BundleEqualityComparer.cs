using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Optimization;

namespace RequireJsNet.Compressor.Helper
{
	/// <summary>
	/// Comparer for BundleFiles and AutoBundleItems
	/// </summary>
	class BundleEqualityComparer : IEqualityComparer<BundleFile>, IEqualityComparer<AutoBundleItem>
	{
		/// <summary>
		/// Compares to BundleFiles for equality
		/// </summary>
		/// <param name="x">The first BundleFile</param>
		/// <param name="y">The second BundleFile</param>
		/// <returns>Whether the BundleFiles are equal</returns>
		public bool Equals(BundleFile x, BundleFile y)
		{
			return x.IncludedVirtualPath.ToLower().Equals(y.IncludedVirtualPath.ToLower());
		}

		/// <inheritdoc />
		public int GetHashCode(BundleFile obj)
		{
			return obj.GetHashCode();
		}

		/// <summary>
		/// Compares to AutoBundleItems for equality
		/// </summary>
		/// <param name="x">The first AutoBundleItem</param>
		/// <param name="y">The second AutoBundleItem</param>
		/// <returns>Whether the AutoBundleItems are equal</returns>
		public bool Equals(AutoBundleItem x, AutoBundleItem y)
		{
			return x.File.ToLower().Equals(y.File.ToLower()) 
				&& x.Directory.ToLower().Equals(y.Directory.ToLower());
		}

		/// <inheritdoc />
		public int GetHashCode(AutoBundleItem obj)
		{
			return obj.GetHashCode();
		}
	}
}
