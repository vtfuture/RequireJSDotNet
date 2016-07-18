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

			var resolvedBaseUrl = UrlHelper.GenerateContentUrl(baseUrl, viewContext.HttpContext);
            var resolvedEntryPointRoot = UrlHelper.GenerateContentUrl(entryPointRoot, viewContext.HttpContext);


            if (resolvedEntryPointRoot != resolvedBaseUrl)
            {
                // entryPointRoot is different from default.
                if ((entryPointRoot.StartsWith("~") || entryPointRoot.StartsWith("/")))
                {
                    // entryPointRoot is defined as root relative, do not use with baseUrl
                    withBaseUrl = false;
                    rootUrl = resolvedEntryPointRoot;
                }
                else
                {
                    // entryPointRoot is defined relative to baseUrl; prepend baseUrl
                    resolvedEntryPointRoot = resolvedBaseUrl + entryPointRoot;
                }                
            }

            var entryPointTemplates = new[]
            {
                "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Action,
                "Controllers/{0}/" + routingInfo.Controller + "/" + routingInfo.Controller + "-" + routingInfo.Action
            };

            var areas = new[]
            {
                routingInfo.Area,
                DefaultArea
            };

            foreach (var entryPointTmpl in entryPointTemplates)
            {
                foreach (var area in areas)
                {
		            var entryPoint = string.Format(entryPointTmpl, area).ToModuleName();
		            var filePath = server.MapPath(entryPointRoot + entryPoint + ".js");

		            if (File.Exists(filePath))
		            {
		                var computedEntry = GetEntryPoint(server, filePath, baseUrl);
		                return withBaseUrl ? computedEntry : rootUrl + computedEntry;
                    }
                }
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
