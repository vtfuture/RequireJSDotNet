using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace RequireJsNet.ResxToJs
{
    internal class JsFileToDictionary
    {
        internal JsFileToDictionary(List<string> lines)
        {
            FileLines = lines;
        }

        private List<string> FileLines { get; set; }

        internal Dictionary<string, string> GetDictionary()
        {
            CleanLines();
            var builder = new StringBuilder();
            FileLines.ForEach(line => builder.Append(line));
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(builder.ToString());
            return deserialized;
        }

        private void CleanLines()
        {
            var cleaned = new List<string>();
            FileLines.ForEach(line =>
            {
                if (!IsSingleLineComment(line))
                {
                    cleaned.Add(RemoveMethodComponents(line));
                }
            });
            FileLines = cleaned;
        }


        private bool IsSingleLineComment(string line)
        {
            return line.Trim().StartsWith("//");
        }

        private string RemoveMethodComponents(string line)
        {
            return line.Replace("define(", string.Empty).Replace("});", "}");
        }
    }
}
