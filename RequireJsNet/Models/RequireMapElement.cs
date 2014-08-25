using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class RequireMapElement
    {
        public string For { get; set; }

        public List<RequireReplacement> Replacements { get; set; }
    }
}
