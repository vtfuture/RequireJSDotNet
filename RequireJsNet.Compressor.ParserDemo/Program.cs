using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.ParserDemo
{
    using System.IO;
    using System.Reflection;

    using Jint.Parser;

    using RequireJsNet.Compressor.Parsing;

    class Program
    {
        static void Main(string[] args)
        {
            var filePath = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                @"Scripts\BForms\Bundles\js\components.js");

            var minimalPath = Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                @"Scripts\minimal.js");

            var text = File.ReadAllText(minimalPath);
            var parser = new JavaScriptParser();
            var program = parser.Parse(text);
            var visitor = new RequireVisitor();
            var result = visitor.Visit(program);
        }


    }
}
