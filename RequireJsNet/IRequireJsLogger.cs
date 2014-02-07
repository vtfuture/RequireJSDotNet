using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet
{
    public interface IRequireJsLogger
    {
        void LogError(string message, string configPath);
    }
}
