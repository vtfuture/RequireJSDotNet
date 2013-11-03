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

namespace RequireJS
{
    public static class RequireJsHtmlHelpers
    {
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
            string configPath = "")
        {
            var setupHtml = new StringBuilder();
            
            var entryPointPath = html.RequireJsEntryPoint();

            //resolve config path
            if (!string.IsNullOrEmpty(configPath) && configPath.StartsWith("~"))
            {
                configPath = html.ViewContext.HttpContext.Server.MapPath(configPath);
            }

            if (entryPointPath != null)
            {
                setupHtml.AppendLine("<script type=\"text/javascript\">");

                setupHtml.AppendLine("var requireConfig = {");
                setupHtml.Append("locale:'" + html.CurrentCulture() + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("pageOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.PageOptions));
                setupHtml.AppendLine(",");
                setupHtml.AppendLine("websiteOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.GlobalOptions));
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("var require = {");
                setupHtml.AppendLine("locale:'" + html.CurrentCulture() + "'");
                setupHtml.Append(",");
                setupHtml.AppendLine("baseUrl:'" + baseUrl + "'");
                setupHtml.Append(",");
                setupHtml.AppendLine("paths:" + html.GetRequireJsPaths(configPath));
                setupHtml.Append(",");
                setupHtml.AppendLine("shim:" + html.GetRequireJsShim(configPath));
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("</script>");

                setupHtml.AppendLine("<script data-main=\"" + entryPointPath + "\" src=\"" + requireUrl + "\">");
                setupHtml.AppendLine("</script>");
            }

            return new MvcHtmlString(setupHtml.ToString());
        }

        /// <summary>
        /// Setup RequireJS to be used in layouts
        /// </summary>
        /// <param name="baseUrl">Scrips folder</param>
        /// <param name="requireUrl">requirejs.js url</param>
        /// <param name="configsList">RequireJS.config files path</param>
        public static MvcHtmlString RenderRequireJsSetup(this HtmlHelper html, string baseUrl, string requireUrl,
            IList<string> configsList)
        {
            var setupHtml = new StringBuilder();

            var entryPointPath = html.RequireJsEntryPoint();

            var configs = MapPath(html, configsList);

            if (entryPointPath != null)
            {
                setupHtml.AppendLine("<script type=\"text/javascript\">");

                setupHtml.AppendLine("var requireConfig = {");
                setupHtml.Append("locale:'" + html.CurrentCulture() + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("pageOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.PageOptions));
                setupHtml.AppendLine(",");
                setupHtml.AppendLine("websiteOptions:" + RequireJsOptions.ConvertToJsObject(html.ViewBag.GlobalOptions));
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("var require = {");
                setupHtml.Append("locale:'" + html.CurrentCulture() + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("baseUrl:'" + baseUrl + "'");
                setupHtml.AppendLine(",");
                setupHtml.Append("paths:" + html.GetRequireJsPaths(configs));
                setupHtml.AppendLine(",");
                setupHtml.AppendLine("shim:" + html.GetRequireJsShim(configs));
                setupHtml.AppendLine("};");

                setupHtml.AppendLine("</script>");

                setupHtml.AppendLine("<script data-main=\"" + entryPointPath + "\" src=\"" + requireUrl + "\">");
                setupHtml.AppendLine("</script>");
            }

            return new MvcHtmlString(setupHtml.ToString());
        }

        public static MvcHtmlString RequireJsEntryPoint(this HtmlHelper html)
        {
            var area = html.ViewContext.RouteData.DataTokens["area"] != null
                ? html.ViewContext.RouteData.DataTokens["area"].ToString()
                : "Root";

            var controller = html.ViewContext.Controller.ValueProvider.GetValue("controller").RawValue as string;
            var action = html.ViewContext.Controller.ValueProvider.GetValue("action").RawValue as string;

            //search for controller/action.js in current area
            var entryPointTmpl = "Controllers/{0}/" + controller + "/" + action;
            var entryPoint = string.Format(entryPointTmpl, area);
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
            entryPointTmpl = "Controllers/{0}/" + controller + "/" + controller + "-" + action;
            entryPoint = string.Format(entryPointTmpl, area);
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

        public static MvcHtmlString GetRequireJsPaths(this HtmlHelper html, string configPath = "")
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.config");
            }

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("RequireJS config not found", configPath);
            }

            var result = new StringBuilder();
            var paths = XDocument.Load(configPath).Descendants("paths").Descendants("path");

            result.Append("{");
            foreach (var item in paths)
            {
                result.AppendFormat("\"{0}\":\"{1}\"{2}", item.Attribute("key").Value.Trim(),
                    item.Attribute("value").Value.Trim(), paths.Last() == item ? "" : ",");
            }
            result.Append("}");

            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString GetRequireJsPaths(this HtmlHelper html, IList<string> configsList)
        {
            var pathList = new List<string>();

            var result = new StringBuilder();
            result.Append("{");
            foreach (var configPath in configsList)
            {
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("RequireJS config not found", configPath);
                }

                var paths = XDocument.Load(configPath).Descendants("paths").Descendants("path");
                foreach (var item in paths)
                {
                    //check unique name
                    var name = item.Attribute("key").Value.Trim();
                    if (pathList.Contains(name))
                    {
                        throw new DuplicateNameException(name + " duplicate path found in " + configPath);
                    }
                    pathList.Add(name);

                    result.AppendFormat("\"{0}\":\"{1}\"{2}", item.Attribute("key").Value.Trim(),
                        item.Attribute("value").Value.Trim(),
                        (paths.Last() == item && configsList.Last() == configPath) ? "" : ",");
                }

            }
            result.Append("}");
            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString GetRequireJsShim(this HtmlHelper html, string configPath = "")
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = html.ViewContext.HttpContext.Server.MapPath("~/RequireJS.config");
            }

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("RequireJS config not found", configPath);
            }

            var result = new StringBuilder();
            var shims = XDocument.Load(configPath).Descendants("shim").Descendants("dependencies");

            result.Append("{");
            foreach (var item in shims)
            {
                result.AppendFormat(" \"{0}\": {1} deps: [", item.Attribute("for").Value.Trim(), "{");
                var deps = item.Descendants("add");
                foreach (var dep in deps)
                {
                    result.AppendFormat("\"{0}\"{1}", dep.Attribute("dependency").Value.Trim(),
                        deps.Last() == dep ? "" : ",");
                }

                var exports = item.Attribute("exports") != null &&
                              !string.IsNullOrEmpty(item.Attribute("exports").Value)
                    ? ", exports: '" + item.Attribute("exports").Value.Trim() + "'"
                    : string.Empty;

                result.AppendFormat("]{0} {1}{2} ", exports, "}", shims.Last() == item ? "" : ",");
            }
            result.Append("}");

            return new MvcHtmlString(result.ToString());
        }

        public static MvcHtmlString GetRequireJsShim(this HtmlHelper html, IList<string> configsList)
        {
            var shimList = new List<string>();
            var result = new StringBuilder();
            result.Append("{");
            foreach (var configPath in configsList)
            {
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("RequireJS config not found", configPath);
                }

                var shims = XDocument.Load(configPath).Descendants("shim").Descendants("dependencies");
                foreach (var item in shims)
                {
                    //check unique name
                    var name = item.Attribute("for").Value.Trim();
                    if (shimList.Contains(name))
                    {
                        throw new DuplicateNameException(name + " duplicate shim found in " + configPath);
                    }
                    shimList.Add(name);

                    result.AppendFormat(" \"{0}\": {1} deps: [", item.Attribute("for").Value.Trim(), "{");
                    var deps = item.Descendants("add");
                    foreach (var dep in deps)
                    {
                        result.AppendFormat("\"{0}\"{1}", dep.Attribute("dependency").Value.Trim(),
                            deps.Last() == dep ? "" : ",");
                    }

                    var exports = item.Attribute("exports") != null &&
                                  !string.IsNullOrEmpty(item.Attribute("exports").Value)
                        ? ", exports: '" + item.Attribute("exports").Value.Trim() + "'"
                        : string.Empty;

                    result.AppendFormat("]{0} {1}{2} ", exports, "}",
                        (shims.Last() == item && configsList.Last() == configPath) ? "" : ",");
                }
            }
            result.Append("}");

            return new MvcHtmlString(result.ToString());
        }

        public static string CurrentCulture(this HtmlHelper html)
        {
            // split the ro-Ro string by '-' so it returns eg. ro / en
            return System.Threading.Thread.CurrentThread.CurrentCulture.Name.Split('-')[0];
        }

        public static Dictionary<string, int> ToJsonDictionary<TEnum>()
        {
            var enumType = typeof (TEnum);
            var names = Enum.GetNames(enumType);
            return Enum.GetNames(enumType).ToDictionary(r => r, r => Convert.ToInt32(Enum.Parse(enumType, r)));
        }

        private static List<string> MapPath(HtmlHelper html, IList<string> configsList)
        {
            var list = new List<string>();
            foreach (var item in configsList)
            {
                if(item.StartsWith("~"))
                {
                    list.Add(html.ViewContext.HttpContext.Server.MapPath(item));
                }
                else
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}