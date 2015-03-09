using RequireJsNet.Compressor.Helper;
using RequireJsNet.EntryPointResolver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RequireJsNet.Examples
{
	public class AreaEntryPointResolver : IEntryPointResolver
	{
		public const string EntryPointRoot = "~/Scripts/";

		/// <summary>
		/// Resolves the EntryPoint based on the view path, while beeing case sensitive
		/// </summary>
		/// <param name="viewContext">the current view context</param>
		/// <param name="entryPointRoot">the requirejsnet entry point. Not relevant</param>
		/// <returns>The new entry point</returns>
		public string Resolve(ViewContext viewContext, string entryPointRoot)
		{
			if(viewContext == null)
			{
				throw new ArgumentNullException("viewContext");
			}

			var viewPath = ((RazorView)viewContext.View).ViewPath;

			// remove extension
			var ext = VirtualPathUtility.GetExtension(viewPath);
			if (ext != null)
			{
				var index = viewPath.LastIndexOf(ext);
				viewPath = viewPath.Remove(index);
			}

			// remove Views directory
			var filePath = viewPath.Replace("/Views", string.Empty);

			// redirect to scripts folder
			filePath = filePath.Replace("~/", EntryPointRoot + "Views/");


			if (!File.Exists(HttpContext.Current.Server.MapPath(filePath + ".js"))) 
			{
				// if there is no view specific script, nothing is returned
				return null;
			}

			return VirtualPathUtility.MakeRelative(EntryPointRoot, filePath);
		}
	}
}