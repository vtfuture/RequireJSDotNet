using Newtonsoft.Json;

namespace RequireJsNet.Helpers
{
    using System.Text;

    internal static class JavaScriptHelpers
    {
        public static string SerializeAsVariable<T>(T obj, string varName)
        {
            var json = JsonConvert.SerializeObject(obj);
            return string.Format("var {0} = {1};", varName, json);
        }

        public static string MethodCall(string methodName, params object[] arguments)
        {
            var argsBuilder = new StringBuilder();
            if (arguments != null)
            {
                for (var i = 0; i < arguments.Length; i++)
                {
                    var serialized = JsonConvert.SerializeObject(arguments[i]);

                    var format = i < arguments.Length - 1 ? "{0}, " : "{0}";

                    argsBuilder.AppendFormat(format, serialized);
                }    
            }
            
            return string.Format("{0}({1});", methodName, argsBuilder.ToString());
        }
    }
}
