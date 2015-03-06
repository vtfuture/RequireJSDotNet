using System.IO;
using System.Web;
using Jint.Parser;
using RequireJsNet.Compressor.AutoDependency.Transformations;
using RequireJsNet.Compressor.Helper;
using RequireJsNet.Compressor.Models;
using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Compressor.Transformations;
using RequireJsNet.Models;
using System.Collections.Generic;
using System.Linq;

namespace RequireJsNet.Compressor.AutoDependency
{
	/// <summary>
	/// Class resolves AMD dependencies of virtual files
	/// </summary>
	internal class ScriptProcessor
	{
		private readonly ConfigurationCollection _configuration;

		private readonly PathResolver _resolver;

		/// <summary>
		/// Creates a new ScriptProcessor
		/// </summary>
		/// <param name="configuration">the require configuration</param>
		/// <param name="resolver">a path resolver</param>
		public ScriptProcessor(ConfigurationCollection configuration, PathResolver resolver)
		{
			_configuration = configuration;
			_resolver = resolver;
		}

		/// <summary>
		/// Resolves the dependencies of an AMD file based on its virtual path and processes the files content
		/// by applying transformations
		/// </summary>
		/// <param name="virtualFilePath">The path to a JavaScript file that follows the AMD format</param>
		/// <returns>A ProcessedFile representing the processed AMD file.</returns>
		public ProcessedFile Process(string virtualFilePath)
		{
			var dependencies = new List<string>();

			var requireFile = new ProcessedFile{IncludedVirtualPath = virtualFilePath};
			var requireFilePath = _resolver.VirtualPathToRequirePath(virtualFilePath);

			var shim = this.GetShim(requireFilePath);

			// get content
			var content = new StreamReader(new FileVirtualPathProvider(
										HttpContext.Current.Server.MapPath("/"))
									.GetFile(virtualFilePath).Open())
								.ReadToEnd();
			// duplicate 
			var text = content;

			// if file is a shim, parsing may be skipped
			if (shim == null)
			{
				// Parse content
				var parser = new JavaScriptParser();
				var program = parser.Parse(content);
				var visitor = new RequireVisitor();
				var result = visitor.Visit(program, virtualFilePath);

				var flattenedResults = result.GetFlattened();

				// ignore require modules
				var deps =
					flattenedResults.SelectMany(r => r.Dependencies)
						.Where(r => !r.StartsWith("i18n"))
						.Except(new List<string> { "require", "module", "exports" });

				dependencies.AddRange(deps);

				var transformations = this.GetTransformations(requireFilePath, result);
				
				transformations.ExecuteAll(ref text);
			}
			else
			{
				// add shim dependencies
				dependencies.AddRange(shim.Dependencies.Select(r => r.Dependency));

				// shim transformation
				var trans = ShimFileTransformation.Create(this.CheckForAlias(_resolver.VirtualPathToRequirePath(virtualFilePath)), dependencies, shim.Exports);
				trans.Execute(ref text);
			}

			requireFile.ProcessedContent = text;
			// ignore duplicates and remove external files from cdn
			requireFile.Dependencies = dependencies.Distinct()
				.Select(r => new AutoBundleItem { File = this.CheckForOriginalModuleName(r) })
				.Where(r => !r.File.StartsWith("//"))
				.ToList();

			return requireFile;
		}

		/// <summary>
		/// Returns the shim entry for a require module
		/// if this module is a shim 
		/// </summary>
		/// <param name="requireModuleName">The module name</param>
		/// <returns>the corresponding shim entry or null, if the module is not a shim</returns>
		private ShimEntry GetShim(string requireModuleName)
		{
			return _configuration.Shim.ShimEntries.FirstOrDefault(r => r.For.ToLower() == requireModuleName.ToLower()
																	|| r.For.ToLower() == this.CheckForAlias(requireModuleName).ToLower());
		}

		/// <summary>
		/// Checks whether a module name is an alias and if so
		/// returns the actual path based on the path section of the configuration  
		/// </summary>
		/// <param name="name">the module name</param>
		/// <returns>the original module name or -if existent- the resolved path</returns>
		private string CheckForOriginalModuleName(string name)
		{
			var result = name;
			var pathEl = _configuration.Paths.PathList.FirstOrDefault(r => r.Key.ToLower() == name.ToLower());
			if (pathEl != null)
			{
				result = pathEl.Value;
			}

			return result;
		}

		/// <summary>
		/// Checks whether a module name has an alias and if so
		/// returns the alias based on the path section of the configuration  
		/// </summary>
		/// <param name="name">the module name</param>
		/// <returns>the original module name or -if existent- the alias</returns>
		private string CheckForAlias(string name)
		{
			var result = name;
			var pathEl = _configuration.Paths.PathList.FirstOrDefault(r => r.Value.ToLower() == name.ToLower());
			if (pathEl != null)
			{
				result = pathEl.Key;
			}

			return result;
		}

		/// <summary>
		/// Returns a collection of transformations to be executed
		/// </summary>
		/// <param name="requirePath">The require path of the file to be transformed</param>
		/// <param name="result">The visitor result for the file</param>
		/// <returns>A transformation collection</returns>
		private TransformationCollection GetTransformations(string requirePath, VisitorResult result)
		{
			var trans = new TransformationCollection();

			if (result.RequireCalls.Any())
			{

				// if there are no define calls but there is at least one require module call, transform that into a define call
				if (!result.RequireCalls.Any(r => r.Type == RequireCallType.Define))
				{
					if (result.RequireCalls.Any(r => r.IsModule))
					{
						var call = result.RequireCalls.FirstOrDefault(r => r.IsModule);
						trans.Add(ToDefineTransformation.Create(call));
						trans.Add(AddIdentifierTransformation.Create(call, this.CheckForAlias(requirePath)));
					}
				}
				else
				{
					var defineCall = result.RequireCalls.FirstOrDefault(r => r.Type == RequireCallType.Define);
					if (string.IsNullOrEmpty(defineCall.Id))
					{
						trans.Add(AddIdentifierTransformation.Create(defineCall, this.CheckForAlias(requirePath)));
					}

					if (defineCall.DependencyArrayNode == null)
					{
						trans.Add(AddEmptyDepsArrayTransformation.Create(defineCall));
					}
				}
			}

			return trans;
		}
	}
}
