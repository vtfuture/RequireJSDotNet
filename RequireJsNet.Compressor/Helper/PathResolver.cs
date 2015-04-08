using System;
using System.IO;
using System.Web;

namespace RequireJsNet.Compressor.Helper
{
	/// <summary>
	/// Resolves virtual and require paths 
	/// </summary>
	public class PathResolver
	{
		/// <summary>
		/// The RequireJs entry point
		/// </summary>
		private readonly string entryPoint;

		private bool throwExceptions;

		/// <summary>
		/// Creates a new path resolver
		/// </summary>
		/// <param name="entryPoint">The RequireJs entry point</param>
		public PathResolver(string entryPoint, bool throwExceptions = true)
		{
			this.entryPoint = entryPoint;
			this.throwExceptions = throwExceptions;
		}

		/// <summary>
		/// Converts a require path to a virtual file path
		/// </summary>
		/// <param name="requirePath">The require path</param>
		/// <param name="appendExtension">Whether to append the .js extension. Usefull when handling directories</param>
		/// <returns>The virtual path if existent</returns>
		public string RequirePathToVirtualPath(string requirePath, bool appendExtension = true)
		{
			if (requirePath.StartsWith("//"))
			{
				if(!throwExceptions)
				{
					// Wenn Exceptions unterdrückt werden, nichts zurück geben
					return null;
				}
				throw new ArgumentException(String.Format("Can not convert CDN path '{0}'", requirePath));
			}

			var path = GetOutputPath(entryPoint, requirePath, appendExtension);

			// get absolute path for existence check
			var absolutePath = HttpContext.Current.Server.MapPath(path);

			// check for existence
			var exists = appendExtension ? File.Exists(absolutePath) : Directory.Exists(absolutePath);

			if(!exists)
			{
				if (!throwExceptions)
				{
					// Wenn Exceptions unterdrückt werden, nichts zurück geben
					return null;
				}

				var message = string.Format("The virtual path '{0}' is not valid", path);
				if (appendExtension)
				{
					throw new FileNotFoundException(message);
				}
				else
				{
					throw new DirectoryNotFoundException(message);
				}
			}

			return path;
		}

		/// <summary>
		/// Converts a virtual file path to a require path
		/// This method cannot handle directories
		/// </summary>
		/// <param name="virtualFilePath">The virtual path to the AMD file</param>
		/// <returns>The require path</returns>
		public string VirtualPathToRequirePath(string virtualFilePath)
		{
			var requirePath = VirtualPathUtility.MakeRelative(entryPoint, virtualFilePath);

			return requirePath.Remove(requirePath.Length - 3);
		}

		/// <summary>
		/// Combines to paths to a complete virtual output path
		/// </summary>
		/// <param name="basePath">The base path has to be a rooted virtual path</param>
		/// <param name="filePath">The file path has to be a path relative to the basepath</param>
		/// <param name="appendExtension">Whether to append the .js extension. Usefull when handling directories</param>
		/// <returns>The complete rooted virtual output path</returns>
		public string GetOutputPath(string basePath, string filePath, bool appendExtension = true)
		{
			var path = VirtualPathUtility.Combine(basePath, filePath);

			if (appendExtension)
			{
				path += ".js";
			}

			return path;
		}
	}
}
