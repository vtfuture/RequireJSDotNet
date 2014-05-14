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
            var filePath = GetPath(@"Scripts\BForms\Bundles\js\components.js");
            var minimalPath = GetPath(@"Scripts\minimal.js");
            var initUIPath = GetPath(@"Scripts\BForms\Plugins\UI\bforms.initUI.js");

            var text = File.ReadAllText(initUIPath);
            var parser = new JavaScriptParser();
            var program = parser.Parse(text);
            var visitor = new RequireVisitor();
            var result = visitor.Visit(program);
        }

        static string GetPath(string relativePath)
        {
            return Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                relativePath);
        }


    }
}
