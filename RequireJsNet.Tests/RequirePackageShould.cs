using RequireJsNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RequireJsNet.Tests
{
    public class RequirePackageShould
    {
        [Fact]
        public void InitializeWithOnlyName()
        {
            var name = "package1";

            var actual = new RequirePackage(name);

            Assert.Equal(name, actual.Name);
            Assert.Equal("main", actual.Main);
            Assert.Equal(null, actual.Location);
        }

        [Fact]
        public void InitializeWithAllProperties()
        {
            var name = "package2";
            var main = "start";
            var location = "/Scripts/CommonJS/package2";

            var actual = new RequirePackage(name, main, location);

            Assert.Equal(name, actual.Name);
            Assert.Equal(main, actual.Main);
            Assert.Equal(location, actual.Location);
        }
    }
}
