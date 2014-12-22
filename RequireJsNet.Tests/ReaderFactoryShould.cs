using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RequireJsNet.Configuration;

using Xunit;

namespace RequireJsNet.Tests
{
    public class ReaderFactoryShould
    {
        [Fact]
        public void ReturnXmlReaderWhenConfigIsXml()
        {
            Assert.IsType<XmlReader>(ReaderFactory.CreateReader("test.xml", null));
            Assert.IsType<XmlReader>(ReaderFactory.CreateReader("test.config", null));
        }

        [Fact]
        public void IgnoreExtensionCase()
        {
            Assert.Equal(ReaderFactory.CreateReader("test.json", null).GetType(), ReaderFactory.CreateReader("test.JsOn", null).GetType());
        }

        public void ReturnJsonReaderWhenConfigIsXml()
        {
            Assert.IsType<JsonReader>(ReaderFactory.CreateReader("test.json", null));
        }
    }
}
