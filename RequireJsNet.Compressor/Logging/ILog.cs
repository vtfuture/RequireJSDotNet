// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;

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
