// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

namespace RequireJsNet.Validation
{
    internal class ConfigValidationError
    {
        public string Message { get; set; }

        public string ElementType { get; set; }

        public string ElementKey { get; set; }

        public string ElementProperty { get; set; }

        public int Index { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
