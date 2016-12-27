using EcmaScript.NET;
using RequireJsNet.Compressor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJSNet.Compressor.Tests.Helpers
{
    public class InMemoryLogAdapter : ILog
    {
        readonly StringBuilder logger;

        public InMemoryLogAdapter()
        {
            logger = new StringBuilder();
        }

        public override string ToString()
        {
            return logger.ToString();
        }

        public void LogMessage(string message)
        {
            logger.AppendLine(message);
        }

        public void LogBoolean(string name, bool value)
        {
            LogMessage(name + ": " + (value ? "Yes" : "No"));
        }

        public void LogError(string message, params object[] messageArgs)
        {
            logger.AppendFormat("ERROR: " + message, messageArgs);
        }

        public void LogErrorFromException(Exception exception)
        {
            this.logger.AppendFormat("EXCEPTION: {0}", exception.Message);
        }

        public void LogErrorFromException(Exception exception, bool showStackTrace)
        {
            this.logger.AppendFormat("EXCEPTION: {0}", showStackTrace ? exception.ToString() : exception.Message);
        }

        public void LogEcmaError(EcmaScriptException ecmaScriptException)
        {
            this.LogError("An error occurred in parsing the Javascript file.");
            if (ecmaScriptException.LineNumber == -1)
            {
                this.LogError("[ERROR] {0} ********", ecmaScriptException.Message);
            }
            else
            {
                var sourceName = string.IsNullOrEmpty(ecmaScriptException.SourceName)
                                     ? string.Empty
                                     : "Source: {1}. " + ecmaScriptException.SourceName;
                this.LogError(
                    "[ERROR] {0} ******** Line: {2}. LineOffset: {3}. LineSource: \"{4}\"",
                    ecmaScriptException.Message,
                    sourceName,
                    ecmaScriptException.LineNumber,
                    ecmaScriptException.ColumnNumber,
                    ecmaScriptException.LineSource);
            }
        }
    }
}
