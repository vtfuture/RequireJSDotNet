using RequireJsNet.Models;

namespace RequireJsNet
{
    internal interface IConfigReader
    {
        string Path { get; }

        ConfigurationCollection ReadConfig();
    }
}
