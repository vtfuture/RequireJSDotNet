using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Configuration
{
    internal class ConfigLoaderOptions
    {
        public bool ProcessBundles { get; set; }

        public bool ProcessAutoBundles { get; set; }

        public bool LoadOverrides { get; set; }
    }
}
