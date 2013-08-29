/*
 * RequireJS.NET
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
                    pageOptions.Add(key, value);
                    break;
                case RequireJsOptionsScope.Global:
                    if (globalOptions.Keys.Contains(key))
                    {
                        globalOptions.Remove(key);
                    }
                    globalOptions.Add(key, value);
                    break;
            }
        }

        public void Add(string key, Dictionary<string, object> value, RequireJsOptionsScope scope = RequireJsOptionsScope.Page, bool clearExisting = false)
        {
            var dictToModify = new Dictionary<string, object>();
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    dictToModify = pageOptions;
                    break;
                case RequireJsOptionsScope.Global:
                    dictToModify = globalOptions;
                    break;
            }

            var existing = dictToModify.FirstOrDefault(r => r.Key == key).Value;
            if (existing != null)
            {
                if (!clearExisting && existing is Dictionary<string, object>)
                {
                    AppendItems(existing as Dictionary<string, object>, value);
                }
                else
                {
                    dictToModify.Remove(key);
                    dictToModify.Add(key, value);
                }
            }
            else
            {
                dictToModify.Add(key, value);
            }
        }

        public object GetByKey(string key, RequireJsOptionsScope scope)
        {
            return scope == RequireJsOptionsScope.Page ? pageOptions.FirstOrDefault(r => r.Key == key)
                                                       : globalOptions.FirstOrDefault(r => r.Key == key);
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
                    controller.ViewBag.PageOptions = pageOptions;
                    break;
                case RequireJsOptionsScope.Global:
                    controller.ViewBag.GlobalOptions = globalOptions;
                    break;
            }
        }

        public void SaveAll()
        {
            Save(RequireJsOptionsScope.Global);
            Save(RequireJsOptionsScope.Page);
        }

        internal static string ConvertToJsObject(Dictionary<string, object> options)
        {
            var config = new StringBuilder();

            config.Append("{");
            foreach (var item in options)
            {
                config.AppendFormat(" {0}: {1}{2} ", item.Key, JsonConvert.SerializeObject(item.Value), options.Last().Equals(item) ? "" : ",");
            }
            config.Append("}");
            return config.ToString();
        }

        private void AppendItems(Dictionary<string, object> to, Dictionary<string, object> from)
        {
            foreach (var item in from)
            {
                var existing = to.FirstOrDefault(r => item.Key == r.Key).Value;
                if (existing != null)
                {
                    to.Remove(item.Key);
                }
                to.Add(item.Key, item.Value);
            }
        }

    }

    public enum RequireJsOptionsScope
    {
        Page,
        Global
    }
}