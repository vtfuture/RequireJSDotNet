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

            var lines = GetScriptLines(text);

            var flattenedResults = result.GetFlattened();

            flattenedResults.ForEach(x => EnsureHasRange(x.ParentNode.Node, lines));

            var reqLines = flattenedResults.Select(x => GetTextFromFullScript(x.ParentNode.Node.Range, text)).ToList();

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

        static string GetTextFromFullScript(int[] range, string fileText)
        {
            return fileText.Substring(range[0], range[1] - range[0]);
        }

        static string GetPath(string relativePath)
        {
            return Path.Combine(
                Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location),
                relativePath);
        }

        private static void EnsureHasRange(SyntaxNode node, List<ScriptLine> lineList)
        {
            if (node.Range != null)
            {
                return;
            }

            var location = node.Location;
            var startLine = lineList[location.Start.Line - 1];
            var endLine = lineList[location.End.Line - 1];
            var startingIndex = startLine.StartingIndex + location.Start.Column;
            var endIndex = endLine.StartingIndex + location.End.Column;
            node.Range = new[] { startingIndex, endIndex };
        }

        static List<ScriptLine> GetScriptLines(string scriptText)
        {
            var currentLineBuilder = new StringBuilder();
            var result = new List<ScriptLine>();
            var currentLine = new ScriptLine();
            
            if (scriptText.Length > 0)
            {
                result.Add(currentLine);    
            }

            for (var i = 0; i < scriptText.Length; i++)
            {
                var currChar = scriptText[i];
                var separatorLength = 1;

                // this is either a legacy maCOs newline, or it will be followed by \n for the windows one
                if (currChar == '\r' || currChar == '\n')
                {
                    if (currChar == '\r' && i < scriptText.Length - 1 && scriptText[i + 1] == '\n')
                    {
                        // skip the next character since it's part of an \r\n sequence
                        i++;
                        separatorLength = 2;
                    }

                    var prevIndex = 0;
                    var prevCount = 0;
                    var prevSeparator = 0;

                    // if we're not still processing the first line, look at the prev line's starting index
                    if (result.Count > 1)
                    {
                        var rpev = result[result.Count - 2];
                        prevIndex = rpev.StartingIndex;
                        prevCount = rpev.LineText.Length;
                        prevSeparator = rpev.NewLineLength;
                    }

                    currentLine.LineText = currentLineBuilder.ToString();
                    currentLine.StartingIndex = prevIndex + prevCount + prevSeparator;
                    currentLine.NewLineLength = separatorLength;
                    currentLine = new ScriptLine();
                    result.Add(currentLine);
                    currentLineBuilder.Clear();
                } 
                else
                {
                    currentLineBuilder.Append(scriptText[i]);
                }
            }

            if (currentLine.LineText == null && currentLine.StartingIndex == 0)
            {
                result.Remove(currentLine);
            }

            return result;
        }
    }
}
