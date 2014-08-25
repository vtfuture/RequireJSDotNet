using System.Collections.Generic;

namespace RequireJsDotNet.Models
{
    internal class RequireMapElement
    {
        public string For { get; set; }

        public List<RequireReplacement> Replacements { get; set; }
    }
}
