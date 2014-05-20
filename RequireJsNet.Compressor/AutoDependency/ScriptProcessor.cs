using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.AutoDependency
{
    using Jint.Parser;
    using Jint.Parser.Ast;

    using RequireJsNet.Compressor.Parsing;

    public class ScriptProcessor
    {
        public ScriptProcessor(string relativeFileName, string scriptText)
        {
            RelativeFileName = relativeFileName;
            OriginalString = scriptText;
        }

        public string OriginalString { get; set; }

        public string ProcessedString { get; set; }

        public string RelativeFileName { get; set; }

        public void Process()
        {
            var parser = new JavaScriptParser();
            var program = parser.Parse(OriginalString);
            var visitor = new RequireVisitor();
            var result = visitor.Visit(program);

            var lines = GetScriptLines(OriginalString);

            var flattenedResults = result.GetFlattened();

            flattenedResults.ForEach(
                x =>
                {
                    EnsureHasRange(x.ParentNode.Node, lines);
                    EnsureHasRange(x.DependencyArrayNode, lines);
                    EnsureHasRange(x.ModuleDefinitionNode, lines);
                    EnsureHasRange(x.ModuleIdentifierNode, lines);
                    EnsureHasRange(x.SingleDependencyNode, lines);
                    var arguments = x.ParentNode.Node.As<CallExpression>().Arguments;
                    foreach (var arg in arguments)
                    {
                        EnsureHasRange(arg, lines);
                    }
                });
        }

        private void EnsureHasRange(SyntaxNode node, List<ScriptLine> lineList)
        {
            if (node == null || node.Range != null)
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

        private List<ScriptLine> GetScriptLines(string scriptText)
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
