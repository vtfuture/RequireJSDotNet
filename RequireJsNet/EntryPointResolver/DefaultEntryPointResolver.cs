using RequireJsNet.Helpers;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace RequireJsNet.EntryPointResolver
{
    public class DefaultEntryPointResolver : IEntryPointResolver
    {
        private const string DefaultEntryPointRoot = "~/Scripts/";
        private const string DefaultArea = "Common";

        public virtual string Resolve(ViewContext viewContext, string baseUrl, string entryPointRoot)
        {
            var routingInfo = viewContext.GetRoutingInfo();
            var rootUrl = string.Empty;
            var withBaseUrl = true;
            var server = viewContext.HttpContext.Server;

            if (String.IsNullOrWhiteSpace(entryPointRoot))
            {
                entryPointRoot = baseUrl;            
            }

            var resolvedEntryPointRoot = UrlHelper.GenerateContentUrl(entryPointRoot, viewContext.HttpContext);

            if (resolvedEntryPointRoot != baseUrl)
            {
                // entryPointRoot is different from default.
                if ((entryPointRoot.StartsWith("~") || entryPointRoot.StartsWith("/")))
                {
                    // entryPointRoot is defined as root relative, do not use with baseUrl
                    withBaseUrl = false;
                    rootUrl = UrlHelper.GenerateContentUrl(entryPointRoot, viewContext.HttpContext);
                }
                else
                {
                    // entryPointRoot is defined relative to baseUrl; prepend baseUrl
                    entryPointRoot = baseUrl + entryPointRoot;
                }                
            }

            // search for controller/action.js in current area
            var entryPointTmpl = "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Action;
            var entryPoint = string.Format(entryPointTmpl, routingInfo.Area).ToModuleName();
            var filePath = server.MapPath(entryPointRoot + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                var computedEntry = GetEntryPoint(server, filePath, baseUrl);
                return withBaseUrl ? computedEntry : rootUrl + computedEntry;
            }

            // search for controller/action.js in common area
            entryPoint = string.Format(entryPointTmpl, DefaultArea).ToModuleName();
            filePath = server.MapPath(entryPointRoot + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                var computedEntry = GetEntryPoint(server, filePath, baseUrl);
                return withBaseUrl ? computedEntry : rootUrl + computedEntry;
            }

            // search for controller/controller-action.js in current area
            entryPointTmpl = "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Controller + "-" + routingInfo.Action;
            entryPoint = string.Format(entryPointTmpl, routingInfo.Area).ToModuleName();
            filePath = server.MapPath(entryPointRoot + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                var computedEntry = GetEntryPoint(server, filePath, baseUrl);
                return withBaseUrl ? computedEntry : rootUrl + computedEntry;
            }

            // search for controller/controller-action.js in common area
            entryPoint = string.Format(entryPointTmpl, DefaultArea).ToModuleName();
            filePath = server.MapPath(entryPointRoot + entryPoint + ".js");

            if (File.Exists(filePath))
            {
                var computedEntry = GetEntryPoint(server, filePath, baseUrl);
                return withBaseUrl ? computedEntry : rootUrl + computedEntry;
            }

            return null;
        }

        private static string GetEntryPoint(HttpServerUtilityBase server, string filePath, string root)
        {

            var fileName = PathHelpers.GetExactFilePath(filePath);
            var folder = server.MapPath(root);
            return PathHelpers.GetRequireRelativePath(folder, fileName);
        }
    }
}
