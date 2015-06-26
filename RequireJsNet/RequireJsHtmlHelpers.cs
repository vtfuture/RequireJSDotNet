// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using RequireJsNet.Configuration;
using RequireJsNet.Helpers;
using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RequireJsNet
{

    public static class RequireJsHtmlHelpers
    {
        /// <summary>
        /// Setup RequireJS to be used in layouts
        /// </summary>
        /// <param name="html">
        /// Html helper.
        /// </param>
        /// <param name="config">
        /// Configuration object for various options.
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString RenderRequireJsSetup(
            this HtmlHelper html,
            RequireRendererConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var entryPointPath = html.RequireJsEntryPoint(config.BaseUrl, config.EntryPointRoot);

            if (entryPointPath == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            if (config.ConfigurationFiles == null || !config.ConfigurationFiles.Any())
            {
                throw new Exception("No config files to load.");
            }

            var processedConfigs = config.ConfigurationFiles.Select(r =>
            {
                var resultingPath = html.ViewContext.HttpContext.MapPath(r);
                PathHelpers.VerifyFileExists(resultingPath);
                return resultingPath;
            }).ToList();

            var resultingConfig = GetCachedOverridenConfig(processedConfigs, config, entryPointPath.ToString());

            var locale = config.LocaleSelector(html);

            var outputConfig = new JsonRequireOutput
            {
                BaseUrl = config.BaseUrl,
                Locale = locale,
                UrlArgs = config.UrlArgs,
                WaitSeconds = config.WaitSeconds,
                Paths = resultingConfig.Paths.PathList.ToDictionary(r => r.Key, r => r.Value),
                Shim = resultingConfig.Shim.ShimEntries.ToDictionary(
                        r => r.For,
                        r => new JsonRequireDeps
                                 {
                                     Dependencies = r.Dependencies.Select(x => x.Dependency).ToList(),
                                     Exports = r.Exports
                                 }),
                Map = resultingConfig.Map.MapElements.ToDictionary(
                         r => r.For,
                         r => r.Replacements.ToDictionary(x => x.OldKey, x => x.NewKey))
            };

            config.ProcessConfig(outputConfig);

            var options = new JsonRequireOptions
            {
                Locale = locale,
                PageOptions = RequireJsOptions.GetPageOptions(html.ViewContext.HttpContext),
                WebsiteOptions = RequireJsOptions.GetGlobalOptions(html.ViewContext.HttpContext)
            };

            config.ProcessOptions(options);

            var configBuilder = new JavaScriptBuilder();
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(options, "requireConfig"));
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(outputConfig, "require"));

            var requireRootBuilder = new JavaScriptBuilder();
            requireRootBuilder.AddAttributesToStatement("src", config.RequireJsUrl);

            var requireEntryPointBuilder = new JavaScriptBuilder();
            requireEntryPointBuilder.AddStatement(
                JavaScriptHelpers.MethodCall(
                "require", 
                (object)new[] { entryPointPath.ToString() }));

            return new MvcHtmlString(
                configBuilder.Render() 
                + Environment.NewLine
                + requireRootBuilder.Render() 
                + Environment.NewLine
                + requireEntryPointBuilder.Render());
        }

        private static HashStore<ConfigurationCollection> configObjectHash = new HashStore<ConfigurationCollection>();

        private static ConfigurationCollection GetCachedOverridenConfig(
            List<string> processedConfigs,
            RequireRendererConfiguration config,
            string entryPointPath)
        {
            if (config.CacheConfigObject)
            {
                return configObjectHash.GetOrSet(
                    ComputeConfigObjectHash(processedConfigs, entryPointPath),
                    () => GetOverridenConfig(processedConfigs, config, entryPointPath));
            }

            return GetOverridenConfig(processedConfigs, config, entryPointPath);
        }

        private static string ComputeConfigObjectHash(List<string> processedConfigs, string entryPointPath)
        {
            return string.Join("|", processedConfigs) + "|" + entryPointPath;
        }

        private static ConfigurationCollection GetOverridenConfig(
            List<string> processedConfigs,
            RequireRendererConfiguration config,
            string entryPointPath)
        {
            var loader = new ConfigLoader(
                processedConfigs,
                config.Logger,
                new ConfigLoaderOptions
                    {
                        LoadOverrides = config.LoadOverrides,
                        CachingPolicy = config.ConfigCachingPolicy
                    });
            var resultingConfig = loader.Get();

            var overrider = new ConfigOverrider();
            overrider.Override(resultingConfig, entryPointPath.ToModuleName());

            return resultingConfig;
        }

        /// <summary>
        /// Returns entry point script relative path
        /// </summary>
        /// <param name="html">
        /// The HtmlHelper instance.
        /// </param>
        /// <param name="root">
        /// Relative root path ex. ~/Scripts/
        /// </param>
        /// <returns>
        /// The <see cref="MvcHtmlString"/>.
        /// </returns>
        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html, string baseUrl, string root)
        {
            var result = RequireJsOptions.ResolverCollection.Resolve(html.ViewContext, baseUrl, root);

            return result != null ? new MvcHtmlString(result) : null;
        }

        public static Dictionary<string, int> ToJsonDictionary<TEnum>()
        {
            var enumType = typeof(TEnum);
            return Enum.GetNames(enumType).ToDictionary(r => r, r => Convert.ToInt32(Enum.Parse(enumType, r)));
        }


        
    }
}