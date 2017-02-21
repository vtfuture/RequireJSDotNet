using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class JsonReader : IConfigReader
    {
        private readonly string path;

        private readonly ConfigLoaderOptions options;

        private readonly IFileReader fileReader;

        private ConfigurationCollection collection;

        public JsonReader(string path, ConfigLoaderOptions options)
        {
            this.path = path;
            this.options = options;
        }

        public JsonReader(string path, ConfigLoaderOptions options, IFileReader reader)
        {
            this.path = path;
            this.options = options;
            this.fileReader = reader;
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public ConfigurationCollection ReadConfig()
        {
            if (collection == null)
            {
                collection = ProcessConfig();
            }

            return collection;
        }

        private ConfigurationCollection ProcessConfig()
        {
            string text;
            if (fileReader == null)
            {
                text = File.ReadAllText(Path);
            }
            else
            {
                text = fileReader.ReadFile(path);
            }

            var collection = new ConfigurationCollection();
            var deserialized = (JObject)JsonConvert.DeserializeObject(text);
            collection.FilePath = Path;
            collection.Paths = GetPaths(deserialized);
            collection.Shim = GetShim(deserialized);
            collection.Map = GetMap(deserialized);

            if (options.ProcessBundles)
            {
                collection.Bundles = GetBundles(deserialized);
            }

            collection.AutoBundles = GetAutoBundles(deserialized);
            collection.Overrides = GetOverrides(deserialized);


            return collection;
        }

        private RequirePaths GetPaths(JObject document)
        {
            var paths = new RequirePaths();
            paths.PathList = new List<RequirePath>();
            if (document != null && document["paths"] != null)
            {
                paths.PathList = document["paths"]
                    .Select(r => requirePathFrom((JProperty)r))
                    .ToList();
            }
            
            return paths;
        }

        private static RequirePath requirePathFrom(JProperty prop)
        {
            var result = new RequirePath(prop.Name);

            if (prop.Value.Type == JTokenType.String)
            {
                result.Add(prop.Value.ToString());
            }
            else if (prop.Value.Type == JTokenType.Array)
            {
                result.Add(prop.Value.Values<string>());
            }
            else
            {
                var pathObj = (JObject)prop.Value;

                var path = pathObj["path"];
                var defaultBundle = pathObj["defaultBundle"];
                if (path == null || defaultBundle == null)
                    throw new ArgumentException("Expected an object with 'path' and 'defaultBundle' but got " + pathObj.ToString());

                if (path.Type == JTokenType.String)
                    result.Add(path.ToString());
                else if (path.Type == JTokenType.Array)
                    result.Add(prop.Value.Values<string>());
                else
                    throw new ArgumentException("Expected 'path' to be string or array, but got " + path.ToString());

                result.DefaultBundle = defaultBundle.ToString();
            }

            return result;
        }

        private RequireShim GetShim(JObject document)
        {
            var shim = new RequireShim();
            shim.ShimEntries = new List<ShimEntry>();
            if (document != null && document["shim"] != null)
            {
                shim.ShimEntries = document["shim"].Select(
                    r =>
                        {
                            var result = new ShimEntry();
                            var prop = (JProperty)r;
                            result.For = prop.Name;
                            var shimObj = (JObject)prop.Value;
                            result.Exports = shimObj["exports"] != null ? shimObj["exports"].ToString() : null;
                            var depArr = (JArray)shimObj["deps"];
                            result.Dependencies = new List<RequireDependency>();
                            if (depArr != null)
                            {
                                result.Dependencies = depArr.Select(dep => new RequireDependency
                                                                               {
                                                                                   Dependency = dep.ToString()
                                                                               })
                                                            .ToList();
                            }

                            return result;
                        })
                        .ToList();                
            }

            return shim;
        }

        private RequireBundles GetBundles(JObject document)
        {
            var bundles = new RequireBundles();
            bundles.BundleEntries = new List<RequireBundle>();
            if (document != null && document["bundles"] != null)
            {
                bundles.BundleEntries = document["bundles"].Select(
                    r =>
                        {
                            var bundle = new RequireBundle();
                            var prop = (JProperty)r;
                            bundle.Name = prop.Name;
                            if (prop.Value is JArray)
                            {
                                bundle.IsVirtual = false;
                                var arr = prop.Value as JArray;
                                bundle.BundleItems = arr.Select(x => new BundleItem { ModuleName = x.ToString() } ).ToList();    
                                
                            }
                            else if(prop.Value is JObject)
                            {
                                var obj = prop.Value as JObject;
                                bundle.OutputPath = obj["outputPath"] != null ? obj["outputPath"].ToString() : null;
                                var inclArr = obj["includes"] as JArray;
                                if (inclArr != null)
                                {
                                    bundle.Includes = inclArr.Select(x => x.ToString()).ToList();
                                }

                                var itemsArr = obj["items"] as JArray;
                                if (itemsArr != null)
                                {
                                    bundle.BundleItems = itemsArr.Select(
                                        x =>
                                            {
                                                var result = new BundleItem();
                                                if (x is JObject)
                                                {
                                                    result.ModuleName = x["path"] != null ? x["path"].ToString() : null;
                                                    result.CompressionType = x["compression"] != null ? x["compression"].ToString() : null;
                                                }
                                                else if (x is JValue && x.Type == JTokenType.String)
                                                {
                                                    result.ModuleName = x.ToString();
                                                }

                                                return result;
                                            }).ToList();
                                }

                                var isVirtual = obj["virtual"];
                                bundle.IsVirtual = isVirtual is JValue 
                                                    && isVirtual.Type == JTokenType.Boolean
                                                    && (bool)((JValue)isVirtual).Value;
                            }

                            return bundle;
                        })
                    .ToList();
            }

            return bundles;
        }

        private RequireMap GetMap(JObject document)
        {
            var map = new RequireMap();
            map.MapElements = new List<RequireMapElement>();
            if (document != null && document["map"] != null)
            {
                map.MapElements = document["map"].Select(
                    r =>
                        {
                            var currentMap = new RequireMapElement();
                            currentMap.Replacements = new List<RequireReplacement>();
                            var prop = (JProperty)r;
                            currentMap.For = prop.Name;
                            var mapDefinition = prop.Value;
                            if (mapDefinition != null)
                            {
                                currentMap.Replacements = mapDefinition.Select(x => new RequireReplacement()
                                                                                        {
                                                                                            OldKey = ((JProperty)x).Name,
                                                                                            NewKey = ((JProperty)x).Value.ToString()
                                                                                        }).ToList();
                            }

                            return currentMap;
                        })
                        .ToList();
            }
            
            return map;
        }

        private List<CollectionOverride> GetOverrides(JObject document)
        {
            var overrideList = new List<CollectionOverride>();
            if (document["overrides"] != null)
            {
                overrideList = document["overrides"].Select(
                    r =>
                        {
                            var overObj = new CollectionOverride();
                            var prop = (JProperty)r;
                            
                            overObj.BundleId = prop.Name;
                            var valObj = prop.Value as JObject;
                            if (valObj != null)
                            {
                                overObj.Map = GetMap(prop.Value as JObject);
                                overObj.Paths = GetPaths(prop.Value as JObject);
                                overObj.Shim = GetShim(prop.Value as JObject);
                                var bundledItems = valObj["bundledScripts"] as JArray;
                                if (bundledItems != null)
                                {
                                    overObj.BundledScripts = bundledItems.Select(x => x.ToString()).ToList();
                                }
                                
                            }

                            return overObj;
                        })
                    .ToList();
            }

            return overrideList;
        }

        private AutoBundles GetAutoBundles(JObject document)
        {
            var autoBundles = new AutoBundles();
            autoBundles.Bundles = new List<AutoBundle>();
            if (document != null && document["autoBundles"] != null)
            {
                autoBundles.Bundles = document["autoBundles"].Select(
                    r =>
                        {
                            var currentBundle = new AutoBundle();
                            var prop = (JProperty)r;
                            currentBundle.Id = prop.Name;
                            var valueObj = prop.Value as JObject;
                            if (valueObj != null)
                            {
                                currentBundle.OutputPath = valueObj["outputPath"] != null ? valueObj["outputPath"].ToString() : null;
                                currentBundle.CompressionType = valueObj["compressionType"] != null ? valueObj["compressionType"].ToString() : null;
                                currentBundle.ContainingConfig = Path;
                                currentBundle.Includes = new List<AutoBundleItem>();
                                if (valueObj["include"] != null)
                                {
                                    currentBundle.Includes = valueObj["include"].Select(x => autoBundleItemFrom(x)).ToList();    
                                }
                                currentBundle.Excludes = new List<AutoBundleItem>();
                                if (valueObj["exclude"] != null)
                                {
                                    currentBundle.Excludes = valueObj["exclude"].Select(x => autoBundleItemFrom(x)).ToList();
                                }
                            }

                            return currentBundle;
                        }).ToList();
            }

            return autoBundles;
        }

        AutoBundleItem autoBundleItemFrom(JToken token)
        {
            var target = new AutoBundleItem();

            var source = token as JObject;
            if (source == null)
                return target;

            target.BundleId = source["bundleId"] != null ? source["bundleId"].ToString() : null;
            target.File = source["file"] != null ? source["file"].ToString() : null;
            target.Directory = source["directory"] != null ? source["directory"].ToString() : null;

            var numberOfDefinedProperties = new[] { target.BundleId, target.File, target.Directory }.Where(x => x != null).Count();
            if (numberOfDefinedProperties != 1)
                throw new ArgumentException("Expected object with exactly one of 'bundleId', 'file' or 'directory' but got " + source.ToString());

            return target;
        }
    }
}
