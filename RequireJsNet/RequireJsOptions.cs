/*
 * RequireJS for .NET
 * Version 1.0.2.0
 * Release Date 26/08/2013
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RequireJS
{
    public class RequireJsOptions
    {
        private readonly Controller controller;
        private readonly Dictionary<string, object> globalOptions;
        private readonly Dictionary<string, object> pageOptions;

        public RequireJsOptions(Controller controller)
        {
            this.controller = controller;

            pageOptions = new Dictionary<string, object>();
            globalOptions = new Dictionary<string, object>();

            //save in case the RequireJsOptions is never used
            this.SaveAll();
        }

        public void Add(string key, object value, RequireJsOptionsScope scope = RequireJsOptionsScope.Page)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    if (pageOptions.Keys.Contains(key))
                    {
                        pageOptions.Remove(key);
                    }
                    pageOptions.Add(key, JsonConvert.SerializeObject(value));
                    break;
                case RequireJsOptionsScope.Global:
                    if (globalOptions.Keys.Contains(key))
                    {
                        globalOptions.Remove(key);
                    }
                    globalOptions.Add(key, JsonConvert.SerializeObject(value));
                    break;
            }
        }

        public void Clear(RequireJsOptionsScope scope)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    pageOptions.Clear();
                    break;
                case RequireJsOptionsScope.Global:
                    globalOptions.Clear();
                    break;
            }
        }

        public void ClearAll()
        {
            pageOptions.Clear();
            globalOptions.Clear();
        }

        public void Save(RequireJsOptionsScope scope)
        {
            //sends options to view using the ViewBag
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    controller.ViewBag.PageOptions = new MvcHtmlString(ConvertToJsObject(pageOptions));
                    break;
                case RequireJsOptionsScope.Global:
                    controller.ViewBag.WebsiteOptions = new MvcHtmlString(ConvertToJsObject(globalOptions));
                    break;
            }
        }

        public void SaveAll()
        {
            controller.ViewBag.PageOptions = new MvcHtmlString(ConvertToJsObject(pageOptions));
            controller.ViewBag.WebsiteOptions = new MvcHtmlString(ConvertToJsObject(globalOptions));
        }

        private static string ConvertToJsObject(Dictionary<string, object> options)
        {
            var config = new StringBuilder();

            config.Append("{");
            foreach (var item in options)
            {
                config.AppendFormat(" {0}: {1}{2} ", item.Key, item.Value, options.Last().Equals(item) ? "" : ",");
            }
            config.Append("}");
            return config.ToString();
        }

    }

    public enum RequireJsOptionsScope
    {
        Page,
        Global
    }
}