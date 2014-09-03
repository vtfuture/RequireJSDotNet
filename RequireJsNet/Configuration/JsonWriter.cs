using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class JsonWriter : IConfigWriter
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
                obj.Paths = GetPaths(conf.Paths.PathList);
            }

            if (conf.Overrides != null && conf.Overrides.Any())
            {
                obj.Overrides = GetOverrides(conf.Overrides);
            }

            if (conf.Shim != null && conf.Shim.ShimEntries != null && conf.Shim.ShimEntries.Any())
            {
                obj.Shim = GetShim(conf.Shim.ShimEntries);
            }

            if (conf.Map != null && conf.Map.MapElements != null && conf.Map.MapElements.Any())
            {
                obj.Map = GetMap(conf.Map.MapElements);
            }

            if (conf.Bundles != null && conf.Bundles.BundleEntries != null && conf.Bundles.BundleEntries.Any())
            {
                obj.Bundles = GetBundles(conf.Bundles.BundleEntries);
            }

            if (conf.AutoBundles != null && conf.AutoBundles.Bundles != null && conf.AutoBundles.Bundles.Any())
            {
                obj.AutoBundles = GetAutoBundles(conf.AutoBundles.Bundles);
            }

            File.WriteAllText(
                Path, 
                JsonConvert.SerializeObject(
                            obj,
                            new JsonSerializerSettings
                            {
                                Formatting = Formatting.Indented,
                                ContractResolver = new CamelCasePropertyNamesContractResolver()
                            }));
        }

        public dynamic GetOverrides(List<CollectionOverride> overrides)
        {
            return overrides.ToDictionary(
                r => r.BundleId,
                r =>
                new
                    {
                        Paths = r.Paths.PathList.ToDictionary(x => x.Key, x => x.Value),
                        BundledScripts = r.BundledScripts
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

                        return new { Path = r.Value, DefaultBundle = r.DefaultBundle };
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
                            obj.Deps = r.Dependencies.Select(x => x.Dependency);
                        }

                        if (!string.IsNullOrEmpty(r.Exports))
                        {
                            obj.Exports = r.Exports;
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
                            obj.IsVirtual = true;
                        }

                        if (!string.IsNullOrEmpty(r.OutputPath))
                        {
                            obj.OutputPath = r.OutputPath;
                        }

                        if (r.BundleItems != null && r.BundleItems.Any())
                        {
                            obj.Items = r.BundleItems.Select(
                                x =>
                                    {
                                        if (string.IsNullOrEmpty(x.CompressionType)
                                            || x.CompressionType.ToLower() == "standard")
                                        {
                                            return (object)x.ModuleName;
                                        }

                                        return new { CompressionType = x.CompressionType, Path = x.ModuleName };
                                    }).ToList();
                        }

                        if (r.Includes != null && r.Includes.Any())
                        {
                            obj.Includes = r.Includes;
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
                            obj.OutputPath = r.OutputPath;
                        }

                        if (r.Includes != null && r.Includes.Any())
                        {
                            obj.Include = AutoBundleContentSelector(r.Includes);
                        }

                        if (r.Excludes != null && r.Excludes.Any())
                        {
                            obj.Exclude = AutoBundleContentSelector(r.Excludes);
                        }

                        return obj;
                    }).ToList();
        }

        public IEnumerable<dynamic> AutoBundleContentSelector(List<AutoBundleItem> items)
        {
            return items.Select(
                                x =>
                                {
                                    dynamic included = new ExpandoObject();
                                    if (!string.IsNullOrEmpty(x.Directory))
                                    {
                                        included.Directory = x.Directory;
                                    }

                                    if (!string.IsNullOrEmpty(x.File))
                                    {
                                        included.File = x.File;
                                    }

                                    if (!string.IsNullOrEmpty(x.BundleId))
                                    {
                                        included.BundleId = x.BundleId;
                                    }

                                    return included;
                                }).ToList();
        }
    }
}
