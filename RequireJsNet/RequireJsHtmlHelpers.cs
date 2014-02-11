/*
 * RequireJS.NET
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */

using System.Data;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System;
using Microsoft.SqlServer.Server;
using RequireJsNet;
using RequireJsNet.Configuration;
using RequireJsNet.Helpers;
using RequireJsNet.Models;

namespace RequireJS
{
    public static class RequireJsHtmlHelpers
    {
        const string DefaultConfigPath = "~/RequireJS.config";
        /// <summary>
        /// Setup RequireJS to be used in layouts
        /// </summary>
        /// <example>
        /// @Html.RenderRequireJsSetup(Url.Content("~/Scripts"), Url.Content("~/Scripts/require.js"), "~/RequireJS.release.config")
        /// </example>
        /// <param name="baseUrl">Scrips folder</param>
        /// <param name="requireUrl">requirejs.js url</param>
        /// <param name="configPath">RequireJS.config server local path</param>
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, string baseUrl, string requireUrl,
            string configPath = "", IRequireJsLogger logger = null)
        {
            return html.RenderRequireJsSetup(baseUrl, requireUrl, new List<string> { configPath }, logger);
        }

        /// <summary>
        /// Setup RequireJS to be used in layouts
        /// </summary>
        /// <param name="baseUrl">Scrips folder</param>
        /// <param name="requireUrl">requirejs.js url</param>
        /// <param name="configsList">RequireJS.config files path</param>
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, string baseUrl, string requireUrl,
            IList<string> configsList, IRequireJsLogger logger = null)
        {
            var entryPointPath = html.RequireJsEntryPoint();

            if (entryPointPath == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            if (!configsList.Any())
            {
                throw new Exception("No config files to load.");
            }
            var processedConfigs = configsList.Select(r =>
            {
                var resultingPath = html.ViewContext.HttpContext.MapPath(r);
                PathHelpers.VerifyFileExists(resultingPath);
                return resultingPath;
            }).ToList();

            var loader = new ConfigLoader(processedConfigs, logger);
            var resultingConfig = loader.Get();
            var outputConfig = new JsonConfig
            {
                BaseUrl = baseUrl,
                Locale = html.CurrentCulture(),
                Paths = resultingConfig.Paths.PathList.ToDictionary(r => r.Key, r => r.Value),
                Shim = resultingConfig.Shim.ShimEntries.ToDictionary(r => r.For, r => new JsonRequireDeps
                {
                    Dependencies = r.Dependencies.Select(x => x.Dependency).ToList(),
                    Exports = r.Exports
                }),
                Map = resultingConfig.Map.MapElements.ToDictionary(r => r.For,
                                                                r => r.Replacements.ToDictionary(x => x.OldKey, x => x.NewKey))
            };

            var options = new JsonRequireOptions
            {
                Locale = html.CurrentCulture(),
                PageOptions = html.ViewBag.PageOptions,
                WebsiteOptions = html.ViewBag.GlobalOptions
            };

            var configBuilder = new JavaScriptBuilder();
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(options, "requireConfig"));
            configBuilder.AddStatement(JavaScriptHelpers.SerializeAsVariable(outputConfig, "require"));

            var requireRootBuilder = new JavaScriptBuilder();
            requireRootBuilder.AddAttributesToStatement("data-main", entryPointPath.ToString());
            requireRootBuilder.AddAttributesToStatement("src", requireUrl);

            return new MvcHtmlString(configBuilder.Render() + requireRootBuilder.Render());
        }

        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html)
        {
            var routingInfo = html.GetRoutingInfo();

            //search for controller/action.js in current area
            var entryPointTmpl = "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Action;
            var entryPoint = string.Format(entryPointTmpl, routingInfo.Area);
            var filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                return new MvcHtmlString(entryPoint);
            }

            //search for controller/action.js in common area
            entryPoint = string.Format(entryPointTmpl, "Common");
            filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                return new MvcHtmlString(entryPoint);
            }

            //search for controller/controller-action.js in current area
            entryPointTmpl = "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Controller + "-" + routingInfo.Action;
            entryPoint = string.Format(entryPointTmpl, routingInfo.Area);
            filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                return new MvcHtmlString(entryPoint);
            }

            //search for controller/controller-action.js in common area
            entryPoint = string.Format(entryPointTmpl, "Common");
            filePath = html.ViewContext.HttpContext.Server.MapPath("~/Scripts/" + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                return new MvcHtmlString(entryPoint);
            }

            return null;
        }



        public static string CurrentCulture(this HtmlHelper html)
        {
            // split the ro-Ro string by '-' so it returns eg. ro / en
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name.Split('-')[0];
        }

        public static Dictionary<string, int> ToJsonDictionary<TEnum>()
        {
            var enumType = typeof(TEnum);
            return Enum.GetNames(enumType).ToDictionary(r => r, r => Convert.ToInt32(Enum.Parse(enumType, r)));
        }

    }
}