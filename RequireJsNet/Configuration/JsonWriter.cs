using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RequireJsNet.Models;
using Newtonsoft.Json.Converters;

namespace RequireJsNet.Configuration
{
	public class JsonWriter : IConfigWriter
	{
		private readonly ConfigLoaderOptions options;

		public JsonWriter(string path, ConfigLoaderOptions options)
		{
			this.options = options;
			Path = path;
		}

		public string Path { get; private set; }

		public void WriteConfig(ConfigurationCollection conf)
		{
			dynamic obj = new ExpandoObject();
			if (conf.Paths != null && conf.Paths.PathList != null && conf.Paths.PathList.Any())
			{
				obj.paths = GetPaths(conf.Paths.PathList);
			}

			if (conf.Overrides != null && conf.Overrides.Any())
			{
				obj.overrides = GetOverrides(conf.Overrides);
			}

			if (conf.Shim != null && conf.Shim.ShimEntries != null && conf.Shim.ShimEntries.Any())
			{
				obj.shim = GetShim(conf.Shim.ShimEntries);
			}

			if (conf.Map != null && conf.Map.MapElements != null && conf.Map.MapElements.Any())
			{
				obj.map = GetMap(conf.Map.MapElements);
			}

			if (conf.Bundles != null && conf.Bundles.BundleEntries != null && conf.Bundles.BundleEntries.Any())
			{
				obj.bundles = GetBundles(conf.Bundles.BundleEntries);
			}

			if (conf.AutoBundles != null && conf.AutoBundles.Bundles != null && conf.AutoBundles.Bundles.Any())
			{
				obj.autoBundles = GetAutoBundles(conf.AutoBundles.Bundles);
			}

			File.WriteAllText(
				Path, 
				JsonConvert.SerializeObject(
							obj,
							Formatting.Indented,
							new KeyValuePairConverter()
							));
		}

		public dynamic GetOverrides(List<CollectionOverride> overrides)
		{
			return overrides.ToDictionary(
				r => r.BundleId,
				r =>
				new
					{
						paths = r.Paths.PathList.ToDictionary(x => x.Key, x => x.Value),
						bundledScripts = r.BundledScripts
					});
		}

		public dynamic GetPaths(List<RequirePath> pathList)
		{
			return pathList.ToDictionary(
				r => r.Key,
				r =>
					{
						if (string.IsNullOrEmpty(r.DefaultBundle))
						{
							return (object)r.Value;
						}

						return new { path = r.Value, defaultBundle = r.DefaultBundle };
					});
		}

		public dynamic GetShim(List<ShimEntry> shimEntries)
		{
			return shimEntries.ToDictionary(
				r => r.For,
				r =>
					{
						dynamic obj = new ExpandoObject();
						if (r.Dependencies != null && r.Dependencies.Any())
						{
							obj.deps = r.Dependencies.Select(x => x.Dependency);
						}

						if (!string.IsNullOrEmpty(r.Exports))
						{
							obj.exports = r.Exports;
						}

						return obj;
					});
		}

		public dynamic GetMap(List<RequireMapElement> mapElements)
		{
			return mapElements.ToDictionary(r => r.For, r => r.Replacements.ToDictionary(x => x.OldKey, x => x.OldKey));
		}

		public dynamic GetBundles(List<RequireBundle> bundles)
		{
			return bundles.ToDictionary(
				r => r.Name,
				r =>
					{
						dynamic obj = new ExpandoObject();

						if (r.IsVirtual)
						{
							obj.isVirtual = true;
						}

						if (!string.IsNullOrEmpty(r.OutputPath))
						{
							obj.outputPath = r.OutputPath;
						}

						if (r.BundleItems != null && r.BundleItems.Any())
						{
							obj.items = r.BundleItems.Select(
								x =>
									{
										if (string.IsNullOrEmpty(x.CompressionType)
											|| x.CompressionType.ToLower() == "standard")
										{
											return (object)x.ModuleName;
										}

										return new { compressionType = x.CompressionType, path = x.ModuleName };
									}).ToList();
						}

						if (r.Includes != null && r.Includes.Any())
						{
							obj.includes = r.Includes;
						}

						return obj;
					});
		}

		public dynamic GetAutoBundles(List<AutoBundle> autoBundles)
		{
			return autoBundles.ToDictionary(
				r => r.Id,
				r =>
					{
						dynamic obj = new ExpandoObject();

						if (!string.IsNullOrEmpty(r.OutputPath))
						{
							obj.outputPath = r.OutputPath;
						}

						if (r.Includes != null && r.Includes.Any())
						{
							obj.include = AutoBundleContentSelector(r.Includes);
						}

						if (r.Excludes != null && r.Excludes.Any())
						{
							obj.exclude = AutoBundleContentSelector(r.Excludes);
						}

						return obj;
					});
		}

		public IEnumerable<dynamic> AutoBundleContentSelector(List<AutoBundleItem> items)
		{
			return items.Select(
								x =>
								{
									dynamic included = new ExpandoObject();
									if (!string.IsNullOrEmpty(x.Directory))
									{
										included.directory = x.Directory;
									}

									if (!string.IsNullOrEmpty(x.File))
									{
										included.file = x.File;
									}

									if (!string.IsNullOrEmpty(x.BundleId))
									{
										included.bundleId = x.BundleId;
									}

									return included;
								}).ToList();
		}
	}
}
