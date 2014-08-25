using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsDotNet.Configuration
{
    using RequireJsDotNet.Models;

    internal interface IConfigWriter
    {
        string Path { get; }

        void WriteConfig(ConfigurationCollection conf);
    }
}
