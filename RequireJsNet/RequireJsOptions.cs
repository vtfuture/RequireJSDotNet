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
using RequireJsNet.EntryPointResolver;

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

        public static readonly RequireEntryPointResolverCollection ResolverCollection = new RequireEntryPointResolverCollection();

        static RequireJsOptions()
        {
            ResolverCollection.Add(new DefaultEntryPointResolver());
        }

        public static Dictionary<string, object> GetGlobalOptions(HttpContextBase context)
        {
            var page = context.Items[GlobalOptionsKey] as Dictionary<string, object>;
            if (page == null)
            {
                context.Items[GlobalOptionsKey] = new Dictionary<string, object>();
            }

            return (Dictionary<string, object>)context.Items[GlobalOptionsKey];
        }

        public static Dictionary<string, object> GetGlobalOptions()
        {
            return GetGlobalOptions(new HttpContextWrapper(GetCurrentContext()));
        }

        public static Dictionary<string, object> GetPageOptions(HttpContextBase context)
        {
            var page = context.Items[PageOptionsKey] as Dictionary<string, object>;
            if (page == null)
            {
                context.Items[PageOptionsKey] = new Dictionary<string, object>();
            }

            return (Dictionary<string, object>)context.Items[PageOptionsKey];
        }

        public static Dictionary<string, object> GetPageOptions()
        {
            return GetPageOptions(new HttpContextWrapper(GetCurrentContext()));
        }
       

        public static void Add(string key, object value, RequireJsOptionsScope scope = RequireJsOptionsScope.Page)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    var pageOptions = GetPageOptions();
                    if (pageOptions.Keys.Contains(key))
                    {
                        pageOptions.Remove(key);
                    }

                    pageOptions.Add(key, value);
                    break;
                case RequireJsOptionsScope.Global:
                    var globalOptions = GetGlobalOptions();
                    if (globalOptions.Keys.Contains(key))
                    {
                        globalOptions.Remove(key);
                    }

                    globalOptions.Add(key, value);
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
                    dictToModify = GetPageOptions();
                    break;
                case RequireJsOptionsScope.Global:
                    dictToModify = GetGlobalOptions();
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
            return scope == RequireJsOptionsScope.Page ? GetPageOptions().FirstOrDefault(r => r.Key == key)
                                                       : GetGlobalOptions().FirstOrDefault(r => r.Key == key);
        }

        public static void Clear(RequireJsOptionsScope scope)
        {
            switch (scope)
            {
                case RequireJsOptionsScope.Page:
                    GetPageOptions().Clear();
                    break;
                case RequireJsOptionsScope.Global:
                    GetGlobalOptions().Clear();
                    break;
            }
        }

        public static void ClearAll()
        {
            Clear(RequireJsOptionsScope.Global);
            Clear(RequireJsOptionsScope.Page);
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

        private static HttpContext GetCurrentContext()
        {
            if (HttpContext.Current == null)
            {
                throw new Exception("HttpContext.Current is null. RequireJsNet needs a HttpContext in order to work.");
            }

            return HttpContext.Current;
        }
    }
}