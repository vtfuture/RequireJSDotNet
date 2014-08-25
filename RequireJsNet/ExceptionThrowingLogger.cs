// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

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
