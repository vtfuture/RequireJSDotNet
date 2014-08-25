namespace RequireJsNet.Configuration
{
    using RequireJsNet.Models;

    internal interface IConfigWriter
    {
        string Path { get; }

        void WriteConfig(ConfigurationCollection conf);
    }
}
