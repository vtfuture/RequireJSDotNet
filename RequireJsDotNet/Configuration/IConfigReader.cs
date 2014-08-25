using RequireJsDotNet.Models;

namespace RequireJsDotNet
{
    internal interface IConfigReader
    {
        string Path { get; }

        ConfigurationCollection ReadConfig();
    }
}
