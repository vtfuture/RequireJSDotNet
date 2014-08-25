using System;

namespace RequireJsNet
{
    internal class ExceptionThrowingLogger : IRequireJsLogger
    {
        public void LogError(string message, string configPath)
        {
            throw new Exception(string.Format("Error in config at {0}: {1}", configPath, message));
        }
    }
}
