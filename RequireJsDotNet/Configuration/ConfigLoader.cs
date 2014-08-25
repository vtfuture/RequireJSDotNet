using System.Collections.Generic;

using RequireJsDotNet.Models;

namespace RequireJsDotNet.Configuration
{
    using System.IO;
    using System.Linq;

    using RequireJsDotNet.Helpers;

    internal class ConfigLoader
    {
        private readonly List<string> paths = new List<string>();

        private readonly IRequireJsLogger logger;

        private readonly ConfigLoaderOptions options; 

        public ConfigLoader(List<string> paths, IRequireJsLogger logger, ConfigLoaderOptions options = null)
        {
            this.paths = paths;
            this.logger = logger ?? new ExceptionThrowingLogger();
            this.options = options ?? new ConfigLoaderOptions();
        }

        public ConfigurationCollection Get()
        {
            ComposeFinalPathList();
            var collectionList = new List<ConfigurationCollection>();
            foreach (var path in paths)
            {
                var reader = ReaderFactory.CreateReader(path, options);
                var config = reader.ReadConfig();
                ValidateCollection(config, path);
                collectionList.Add(config);
            }

            var configMerger = new ConfigMerger(collectionList, options);
            var merged = configMerger.GetMerged();
            ValidateCollection(merged, null);
            return merged;
        }

        private void ComposeFinalPathList()
        {
            if (!options.LoadOverrides)
            {
                return;
            }

            var potential = this.paths.Select(r => PathHelpers.GetOverridePath(r)).ToList();
            paths.AddRange(potential.Where(r => File.Exists(r)));
        }

        private void ValidateCollection(ConfigurationCollection collection, string path)
        {
            var validator = new ConfigValidator(collection);
            validator.GetErrors().ForEach(x => logger.LogError(x.Message, path));
        }
    }
}
