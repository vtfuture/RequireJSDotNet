using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Models;

namespace RequireJsNet
{
    internal interface IConfigReader
    {
        string Path { get; }
        ConfigurationCollection ReadConfig();
    }
}
