using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using EcmaScript.NET;

namespace RequireJsNet.Compressor
{
    public interface ILog
    {
        void LogMessage(string message);

        void LogBoolean(string name, bool value);

        void LogError(string message, params object[] messageArgs);

        void LogErrorFromException(Exception exception);

        void LogErrorFromException(Exception exception, bool showStackTrace);

        void LogEcmaError(EcmaScriptException ecmaScriptException);
    }
}
