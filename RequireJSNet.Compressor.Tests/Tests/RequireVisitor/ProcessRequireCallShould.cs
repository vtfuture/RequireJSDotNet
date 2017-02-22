using Jint.Parser;
using System.Linq;
using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Compressor.Tests.Helpers;
using RequireJsNet.Configuration;
using RequireJsNet.Models;
using System.Collections.Generic;
using Xunit;

namespace RequireJSNet.Compressor.Tests
{
    public class ProcessRequireCallShould
    {
        //[Fact]
        //public void ResolveSingleDependencyWithoutCallback()
        //{
        //    var expected = new[] { "abc" };

        //    var script = "require('abc');";
        //    var actual = getDependenciesFrom(script);

        //    Assert.Equal(expected, actual);
        //}

        [Fact]
        public void ResolveDependenciesWithoutCallback()
        {
            var expected = new[] { "abc", "def" };

            var script = "require(['abc', 'def']);";
            var actual = getDependenciesFrom(script);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ResolveDependenciesWithCallback()
        {
            var expected = new[] { "ghi", "jkl" };

            var script = "require(['ghi', 'jkl'], function(first){ console.log('done'); });";
            var actual = getDependenciesFrom(script);

            Assert.Equal(expected, actual);
        }

        //[Fact]
        //public void ResolveDependenciesWithCallbackAndErrback()
        //{
        //    var expected = new[] { "mno", "pqr" };

        //    var script = @"
        //        require(['mno', 'pqr'], function(){ 
        //            console.log('done'); 
        //        }, function() { 
        //            console.error('inside errback'); 
        //        });
        //    ";
        //    var actual = getDependenciesFrom(script);

        //    Assert.Equal(expected, actual);
        //}



        private static List<string> getDependenciesFrom(string script)
        {
            return visit(script).Single().Dependencies;
        }

        private static List<RequireCall> visit(string script)
        {
            var parser = new JavaScriptParser();
            var program = parser.Parse(script);
            var visitor = new RequireVisitor();
            var results = visitor.Visit(program, "ProcessRequireCallShouldScript.js").GetFlattened();
            return results;
        }
    }
}
