using System.Collections.Generic;

namespace RequireJsNet.Models
{
    internal class ShimEntry
    {
        public string For { get; set; }

        public string Exports { get; set; }

        public List<RequireDependency> Dependencies { get; set; } 
    }
}
