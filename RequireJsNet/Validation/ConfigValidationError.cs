using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
