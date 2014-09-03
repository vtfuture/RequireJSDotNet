using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Xunit;

namespace RequireJsNet.Tests.Extensions
{
    public static class CustomAssert
    {
        public static void JsonEquals<T>(T expected, T actual)
        {
            Assert.Equal(JsonConvert.SerializeObject(expected), JsonConvert.SerializeObject(actual));
        }
    }
}
