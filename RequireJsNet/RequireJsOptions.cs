// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RequireJsNet
{
    public enum RequireJsOptionsScope
    {
        Page,
        Global
    }

    public static class RequireJsOptions
    {
        private const string GlobalOptionsKey = "globalOptions";

        private const string PageOptionsKey = "pageOptions";

        private static Dictionary<string, object> GlobalOptions 
        {
            get
            {
                var global = CurrentContext.Items[GlobalOptionsKey] as Dictionary<string, object>;
                if (global == null)
                {
                    global = new Dictionary<string, object>();
                    CurrentContext.Items[GlobalOptionsKey] = global;
                }

                return global;
            }
        }

        private static Dictionary<string, object> PageOptions
        {
            get
            {
                var page = CurrentContext.Items[PageOptionsKey] as Dictionary<string, object>;
                if (page == null)
                {
                    page = new Dictionary<string, object>();
                    CurrentContext.Items[GlobalOptionsKey] = page;
                }

                return page;
            }
        }

        private static HttpContext CurrentContext
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    throw new Exception("HttpContext.Current is null. RequireJsNet needs a HttpContext in order to work.");
                }

                return HttpContext.Current;
            }
        }

        public static Dictionary<string, object> GetGlobalOptions(HttpContextBase context)
        {
            return CurrentContext.Items[GlobalOptionsKey] as Dictionary<string, object>
                   ?? new Dictionary<string, object>();
        }

        public static Dictionary<string, object> GetPageOptions(HttpContextBase context)
        {
            return CurrentContext.Items[PageOptionsKey] as Dictionary<string, object>
                   ?? new Dictionary<string, object>();
        }
       

        public static void Add(string key, object value, RequireJsOptionsScope scope = RequireJsOptionsScope.Page)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    if (PageOptions.Keys.Contains(key))
                    {
                        PageOptions.Remove(key);
                    }

                    PageOptions.Add(key, value);
                    break;
                case RequireJsOptionsScope.Global:
                    if (GlobalOptions.Keys.Contains(key))
                    {
                        GlobalOptions.Remove(key);
                    }

                    GlobalOptions.Add(key, value);
                    break;
            }
        }


        public static void Add(
            string key,
            Dictionary<string, object> value,
            RequireJsOptionsScope scope = RequireJsOptionsScope.Page,
            bool clearExisting = false)
        {
            var dictToModify = new Dictionary<string, object>();
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    dictToModify = PageOptions;
                    break;
                case RequireJsOptionsScope.Global:
                    dictToModify = GlobalOptions;
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

        public static object GetByKey(string key, RequireJsOptionsScope scope)
        {
            return scope == RequireJsOptionsScope.Page ? PageOptions.FirstOrDefault(r => r.Key == key)
                                                       : GlobalOptions.FirstOrDefault(r => r.Key == key);
        }

        public static void Clear(RequireJsOptionsScope scope)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    PageOptions.Clear();
                    break;
                case RequireJsOptionsScope.Global:
                    GlobalOptions.Clear();
                    break;
            }
        }

        public static void ClearAll()
        {
            PageOptions.Clear();
            GlobalOptions.Clear();
        }
        

        private static void AppendItems(Dictionary<string, object> to, Dictionary<string, object> from)
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
}