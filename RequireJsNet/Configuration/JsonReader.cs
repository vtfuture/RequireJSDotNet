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
            JObject deserialized;
            try
            {
                deserialized = (JObject)JsonConvert.DeserializeObject(text);
            }
            catch (JsonReaderException ex)
            {
                throw new JsonReaderException($"{ex.Message} File: {Path}", ex);
            }
            collection.FilePath = Path;
            collection.Paths = GetPaths(deserialized);
            collection.Packages = GetPackages(deserialized);
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
            string parseSection = "paths";
            if (document != null && document[parseSection] != null)
            {
                JToken pathParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                paths.PathList = pathParent
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

        private RequirePackages GetPackages(JObject document)
        {
            var packages = new RequirePackages();
            packages.PackageList = new List<RequirePackage>();
            string parseSection = "packages";
            if (document != null && document[parseSection] != null)
            {
                JToken packagesParent = JsonParseOrThrow<JArray>(document[parseSection], parseSection, Path, null);
                packages.PackageList = packagesParent
                    .Select(r => requirePackageFrom(r, parseSection))
                    .ToList();
            }

            return packages;
        }

        private RequirePackage requirePackageFrom(JToken token, string parseSectionHint)
        {
            if (token is JValue)
            {
                var name = ((JValue)token).Value<string>();
                return new RequirePackage(name);
            }
            else
            {
                var name = JsonParseStringOrThrow((JObject)token, "name", parseSectionHint, Path);
                var main = JsonParseStringOrThrow((JObject)token, "main", parseSectionHint, Path);
                var location = JsonParseStringOrThrow((JObject)token, "location", parseSectionHint, Path);
                return new RequirePackage(name, main, location);
            }
        }

        /// <summary>
        /// Parses element <paramref name="wantedInputName"/> of <paramref name="inputObject"/> and returns as string (or null).
        /// Throws detailled error message in case of error.
        /// </summary>
        /// <param name="inputObject">parent object which is to be parsed</param>
        /// <param name="wantedInputName">name of child object which is to be parsed</param>
        /// <param name="parseSectionHint">name of parent section containing <paramref name="inputToken"/> as hint for user. Only used for error message. Usually format as 'parent.child'</param>
        /// <param name="filePath">path to file being parsed. Only used for error message.</param>
        /// <param name="parentToken">outer token. Only used for error message. May be null.</param>
        /// <returns>null or content of <paramref name="wantedInputName"/> object as string</returns>
        private string JsonParseStringOrThrow(JObject inputObject, string wantedInputName, string parseSectionHint, string filePath)
        {
            return JsonParseOrThrow<JValue>(inputObject[wantedInputName], $"{parseSectionHint}.{wantedInputName}", filePath, inputObject)?.ToString();
        }

        /// <summary>
        /// Parses element <paramref name="wantedInputName"/> of <paramref name="inputObject"/> and returns as JSON array (or null).
        /// Throws detailled error message in case of error.
        /// </summary>
        /// <param name="inputObject">parent object which is to be parsed</param>
        /// <param name="wantedInputName">name of child object which is to be parsed</param>
        /// <param name="parseSectionHint">name of parent section containing <paramref name="inputToken"/> as hint for user. Only used for error message. Usually format as 'parent.child'</param>
        /// <param name="filePath">path to file being parsed. Only used for error message.</param>
        /// <param name="parentToken">outer token. Only used for error message. May be null.</param>
        /// <returns>null or content of <paramref name="wantedInputName"/> object as JSON array</returns>
        private JArray JsonParseArrayOrThrow(JObject inputObject, string wantedInputName, string parseSectionHint, string filePath)
        {
            return JsonParseOrThrow<JArray>(inputObject[wantedInputName], $"{parseSectionHint}.{wantedInputName}", filePath, inputObject);
        }


        /// <summary>
        /// Parses input as specified by generics parameter and returns as specified type. Or throws detailled error message in case of error.
        /// </summary>
        /// <typeparam name="T">wanted return type: JValue for string, JArray for array, JObject for JSON object</typeparam>
        /// <param name="inputToken">token which is to be parsed</param>
        /// <param name="parseSectionHint">name of parent section containing <paramref name="inputToken"/> as hint for user. Only used for error message Usually format as 'parent.child.subchild'.</param>
        /// <param name="filePath">path to file being parsed. Only used for error message.</param>
        /// <param name="parentToken">outer token. Only used for error message. May be null.</param>
        /// <returns><paramref name="inputToken"/> as JSON type</returns>
        private T JsonParseOrThrow<T>(JToken inputToken, string parseSectionHint, string filePath, JToken parentToken)
        {
            T jsonOutput;
            try
            {
                jsonOutput = (T)Convert.ChangeType(inputToken, typeof(T));
            }
            catch (InvalidCastException ex)
            {
                //'Object must implement IConvertible.' or
                //'Unable to cast object of type 'Newtonsoft.Json.Linq.JValue' to type 'Newtonsoft.Json.Linq.JObject'.'
                string errMsg = $"Found type '{inputToken.GetType().FullName}' but expected '{typeof(T).FullName}' for token '{inputToken}'. Parent token: '{parentToken}'";
                throw new JsonReaderException($"Error in section '{parseSectionHint}' in file '{System.IO.Path.GetFileName(filePath)}': {errMsg}", ex);
            }
            return jsonOutput;
        }

        private RequireShim GetShim(JObject document)
        {
            var shim = new RequireShim();
            shim.ShimEntries = new List<ShimEntry>();
            string parseSection = "shim";
            if (document != null && document[parseSection] != null)
            {
                JToken shimParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                shim.ShimEntries = shimParent.Select(
                    r =>
                        {
                            var result = new ShimEntry();
                            var prop = (JProperty)r;
                            result.For = prop.Name;
                            string parseSectionHint = parseSection + "." + prop.Name;
                            JObject shimObj = JsonParseOrThrow<JObject>(prop.Value, parseSectionHint, Path, r);
                            result.Exports = JsonParseStringOrThrow(shimObj, "exports", parseSectionHint, Path);
                            JArray depArr = JsonParseArrayOrThrow(shimObj, "deps", parseSectionHint, Path);
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
            string parseSection = "bundles";
            if (document != null && document[parseSection] != null)
            {
                JToken bundlesParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                bundles.BundleEntries = bundlesParent.Select(
                    r =>
                        {
                            var bundle = new RequireBundle();
                            var prop = (JProperty)r;
                            bundle.Name = prop.Name;
                            string parseSectionHint = $"{parseSection}.{prop.Name}";
                            if (prop.Value is JArray)
                            {
                                bundle.IsVirtual = false;
                                JArray arr = JsonParseOrThrow<JArray>(prop.Value, parseSectionHint, Path, null);
                                bundle.BundleItems = arr.Select(x => new BundleItem { ModuleName = x.ToString() } ).ToList();    
                                
                            }
                            else if(prop.Value is JObject)
                            {
                                var obj = prop.Value as JObject;
                                bundle.OutputPath = JsonParseStringOrThrow(obj, "outputPath", parseSectionHint, Path);
                                JArray inclArr = JsonParseArrayOrThrow(obj, "includes", parseSectionHint, Path);
                                if (inclArr != null)
                                {
                                    bundle.Includes = inclArr.Select(x => x.ToString()).ToList();
                                }

                                JArray itemsArr = JsonParseArrayOrThrow(obj, "items", parseSectionHint, Path);
                                if (itemsArr != null)
                                {
                                    string parseSectionHint2 = $"{parseSectionHint}.items";
                                    bundle.BundleItems = itemsArr.Select(
                                        x =>
                                            {
                                                var result = new BundleItem();
                                                if (x is JObject)
                                                {
                                                    result.ModuleName = JsonParseStringOrThrow(x as JObject, "path", parseSectionHint2, Path);
                                                    result.CompressionType = JsonParseStringOrThrow(x as JObject, "compression", parseSectionHint2, Path);
                                                }
                                                else if (x is JValue && x.Type == JTokenType.String)
                                                {
                                                    result.ModuleName = x.ToString();
                                                }

                                                return result;
                                            }).ToList();
                                }

                                var isVirtual = obj["virtual"];
                                if (isVirtual != null && !(isVirtual is JValue)) {
                                    throw new JsonReaderException($"Element 'virtual' in {parseSectionHint} is expected to be 'JValue'; found instead: '{isVirtual.GetType()}'. Value: {isVirtual}");
                                }
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
            string parseSection = "map";
            if (document != null && document[parseSection] != null)
            {
                JToken mapParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                map.MapElements = mapParent.Select(
                    r =>
                        {
                            var currentMap = new RequireMapElement();
                            currentMap.Replacements = new List<RequireReplacement>();
                            var prop = (JProperty)r;
                            currentMap.For = prop.Name;
                            JToken mapDefinition = JsonParseOrThrow<JObject>(prop.Value, parseSection, Path, null);
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
            string parseSection = "overrides";
            if (document[parseSection] != null)
            {
                JToken overridesParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                overrideList = overridesParent.Select(
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
                                JArray bundledItems = JsonParseArrayOrThrow(valObj, "bundledScripts", parseSection, Path);
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
            string parseSection = "autoBundles";
            if (document != null && document[parseSection] != null)
            {
                JToken autoBundlesParent = JsonParseOrThrow<JObject>(document[parseSection], parseSection, Path, null);
                autoBundles.Bundles = autoBundlesParent.Select(
                    r =>
                        {
                            var currentBundle = new AutoBundle();
                            var prop = (JProperty)r;
                            currentBundle.Id = prop.Name;
                            string parseSectionHint = parseSection + "." + prop.Name;
                            var valueObj = prop.Value as JObject;
                            if (valueObj != null)
                            {
                                currentBundle.OutputPath = JsonParseStringOrThrow(valueObj, "outputPath", parseSectionHint, Path);
                                currentBundle.CompressionType = JsonParseStringOrThrow(valueObj, "compressionType", parseSectionHint, Path);
                                currentBundle.ContainingConfig = Path;
                                currentBundle.Includes = new List<AutoBundleItem>();
                                if (valueObj["include"] != null)
                                {
                                    JToken includeParent = JsonParseOrThrow<JArray>(valueObj["include"], parseSection, Path, null);
                                    currentBundle.Includes = includeParent.Select(x => autoBundleItemFrom(x, parseSectionHint)).ToList();    
                                }
                                currentBundle.Excludes = new List<AutoBundleItem>();
                                if (valueObj["exclude"] != null)
                                {
                                    JToken excludeParent = JsonParseOrThrow<JArray>(valueObj["exclude"], parseSection, Path, null);
                                    currentBundle.Excludes = excludeParent.Select(x => autoBundleItemFrom(x, parseSectionHint)).ToList();
                                }
                            }

                            return currentBundle;
                        }).ToList();
            }

            return autoBundles;
        }

        AutoBundleItem autoBundleItemFrom(JToken token, string parseSectionHint)
        {
            var target = new AutoBundleItem();

            var source = token as JObject;
            if (source == null)
                return target;

            target.BundleId = JsonParseStringOrThrow(source, "bundleId", parseSectionHint, Path);
            target.File = JsonParseStringOrThrow(source, "file", parseSectionHint, Path);
            target.Directory = JsonParseStringOrThrow(source, "directory", parseSectionHint, Path);

            var numberOfDefinedProperties = new[] { target.BundleId, target.File, target.Directory }.Where(x => x != null).Count();
            if (numberOfDefinedProperties != 1)
                throw new ArgumentException("Expected object with exactly one of 'bundleId', 'file' or 'directory' but got " + source.ToString());

            return target;
        }
    }
}
