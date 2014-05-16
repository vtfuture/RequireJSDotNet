using System.Collections.Generic;

using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal class ConfigLoader
    {
        private readonly IList<string> paths = new List<string>();

        private readonly IRequireJsLogger logger;

        private readonly ConfigLoaderOptions options; 

        public ConfigLoader(IList<string> paths, IRequireJsLogger logger, ConfigLoaderOptions options = null)
        {
            this.paths = paths;
            this.logger = logger ?? new ExceptionThrowingLogger();
            this.options = options ?? new ConfigLoaderOptions();
        }

        public ConfigurationCollection Get()
        {
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

        private void ValidateCollection(ConfigurationCollection collection, string path)
        {
            var validator = new ConfigValidator(collection);
            validator.GetErrors().ForEach(x => logger.LogError(x.Message, path));
        }
    }
}
