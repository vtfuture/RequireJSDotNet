using RequireJsNet.Models;

namespace RequireJsNet.Configuration
{
    internal interface IConfigReader
    {
        string Path { get; }

        ConfigurationCollection ReadConfig();
    }
}
