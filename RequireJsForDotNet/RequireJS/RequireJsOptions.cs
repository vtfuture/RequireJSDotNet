/*
 * RequireJS for .NET
 * Version 1.0.0.1
 * Release Date 10/06/0213
 * Copyright Stefan Prodan
 *   http://stefanprodan.eu
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 */
using System;
using System.IO;
using System.Xml.Linq;
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
        private readonly Dictionary<string, object> websiteOptions;
        private readonly Dictionary<string, object> pageOptions;

        public RequireJsOptions(Controller controller)
        {
            this.controller = controller;

            pageOptions = new Dictionary<string, object>();
            websiteOptions = new Dictionary<string, object>();

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
                case RequireJsOptionsScope.Website:
                    if (websiteOptions.Keys.Contains(key))
                    {
                        websiteOptions.Remove(key);
                    }
                    websiteOptions.Add(key, JsonConvert.SerializeObject(value));
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
                case RequireJsOptionsScope.Website:
                    websiteOptions.Clear();
                    break;
            }
        }

        public void ClearAll()
        {
            pageOptions.Clear();
            websiteOptions.Clear();
        }

        public void Save(RequireJsOptionsScope scope)
        {
            //sends options to view using the ViewBag
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    controller.ViewBag.PageOptions = new MvcHtmlString(ConvertToJsObject(pageOptions));
                    break;
                case RequireJsOptionsScope.Website:
                    controller.ViewBag.WebsiteOptions = new MvcHtmlString(ConvertToJsObject(websiteOptions));
                    break;
            }
        }

        public void SaveAll()
        {
            controller.ViewBag.PageOptions = new MvcHtmlString(ConvertToJsObject(pageOptions));
            controller.ViewBag.WebsiteOptions = new MvcHtmlString(ConvertToJsObject(websiteOptions));
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
        Website
    }
}