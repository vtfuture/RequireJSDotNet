using System.Web.Mvc;

namespace RequireJsNet.EntryPointResolver
{
    public interface IEntryPointResolver
    {
        string Resolve(ViewContext viewContext, string baseUrl, string entryPointRoot);
    }
}
