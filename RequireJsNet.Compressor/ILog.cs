using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor
{
    public interface ILog
    {
        void LogMessage(string message);

        void LogBoolean(string name, bool value);

        void LogError(string message, params object[] messageArgs);

        void LogErrorFromException(Exception exception);

        void LogErrorFromException(Exception exception, bool showStackTrace);
    }
}
