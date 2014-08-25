namespace RequireJsDotNet.Models
{
    using System.Collections.Generic;

    internal class ConfigurationCollection
    {
        public string FilePath { get; set; }

        public RequirePaths Paths { get; set; }

        public RequireShim Shim { get; set; }

        public RequireMap Map { get; set; }

        public RequireBundles Bundles { get; set; }

        public AutoBundles AutoBundles { get; set; }

        public List<CollectionOverride> Overrides { get; set; }
    }
}
