namespace RequireJsDotNet
{
    public interface IRequireJsLogger
    {
        void LogError(string message, string configPath);
    }
}
