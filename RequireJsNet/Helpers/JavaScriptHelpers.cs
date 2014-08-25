// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System.Text;

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
