using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.ParserDemo
{
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using Jint.Parser;
    using Jint.Parser.Ast;

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

            //var scriptLines = 
            //var requireLines = new List<string>();
            //foreach (var requireCall in result.RequireCalls)
            //{
            //    var pnode = requireCall.ParentNode.Node.As<CallExpression>();
            //    var node = pnode.Arguments.ElementAt(1).As<ArrayExpression>();
            //    requireLines.Add(GetTextFromLines(
            //        pnode.Location.Start.Column,
            //        pnode.Location.Start.Line,
            //        pnode.Location.End.Column,
            //        pnode.Location.End.Line,
            //        lines));
            //}

            var a = 5;
        }

        static string GetTextFromLines(int startCol, int startLine, int endCol, int endLine, List<string> lines)
        {
            startLine = startLine - 1;
            endLine = endLine - 1;
            if (startLine == endLine)
            {
                return GetTextFromSingleLine(startCol, endCol, lines[startLine]);
            }

            var builder = new StringBuilder();

            // take only what we need from the first line
            builder.AppendLine(GetTextFromSingleLine(startCol, lines[startLine].Length, lines[startLine]));
            for (var i = startLine + 1; i < endLine; i++)
            {
                builder.AppendLine(lines[i]);
            }

            builder.AppendLine(GetTextFromSingleLine(0, endCol, lines[endLine]));
            return builder.ToString();
        }

        static string GetTextFromSingleLine(int startIndex, int endIndex, string line)
        {
            return line.Substring(startIndex, endIndex - startIndex);
        }

        static string GetPath(string relativePath)
        {
            return Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                relativePath);
        }

        static void EnsureHasRange(SyntaxNode node, List<string> lineList)
        {
            if (node.Range != null)
            {
                return;
            }

        }

        static List<ScriptLine> GetScriptLines(string scriptText)
        {

            for (var i = 0; i < scriptText.Length; i++)
            {
                
            }

            var lines = Regex.Split(scriptText, "\r\n|\r|\n").ToList();
            var scriptLines = new List<ScriptLine>();
            ScriptLine prevLine = null;
            foreach (var line in lines)
            {
                // add an extra character for the previous line to account for line endings
                var prevCount = prevLine == null ? 0 : prevLine.StartingIndex + prevLine.LineText.Length + 1;
                var currentLine = new ScriptLine { LineText = line, StartingIndex = prevCount };
                scriptLines.Add(currentLine);
                prevLine = currentLine;
            }

            return null;
        }
    }
}
