using Newtonsoft.Json;

namespace RequireJsNet.Helpers
{
    internal static class JavaScriptHelpers
    {
        public static string SerializeAsVariable<T>(T obj, string varName)
        {
            var json = JsonConvert.SerializeObject(obj);
            return string.Format("var {0} = {1};", varName, json);
        }
    }
}
