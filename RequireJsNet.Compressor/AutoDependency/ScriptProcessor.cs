// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jint.Parser;
using Jint.Parser.Ast;

using RequireJsNet.Compressor.AutoDependency.Transformations;
using RequireJsNet.Compressor.Parsing;
using RequireJsNet.Compressor.Transformations;
using RequireJsNet.Helpers;
using RequireJsNet.Models;

namespace RequireJsNet.Compressor.AutoDependency
{
    internal class ScriptProcessor
    {
        private readonly ConfigurationCollection configuration;

        public ScriptProcessor(string relativeFileName, string scriptText, ConfigurationCollection configuration)
        {
            RelativeFileName = relativeFileName;
            OriginalString = scriptText;
            this.configuration = configuration;
        }

        public string OriginalString { get; set; }

        public string ProcessedString { get; set; }

        public string RelativeFileName { get; set; }

        public List<string> Dependencies { get; set; }

        public void Process()
        {
            try
            {
                var parser = new JavaScriptParser();
                var program = parser.Parse(OriginalString);
                var visitor = new RequireVisitor();
                var result = visitor.Visit(program, RelativeFileName);

                var lines = GetScriptLines(OriginalString);

                var flattenedResults = result.GetFlattened();

                var deps =
                    flattenedResults.SelectMany(r => r.Dependencies)
                        .Where(r => !r.Contains("!"))
                        .Except(new List<string> { "require", "module", "exports" });

                Dependencies = deps.Select(r => GetModulePath(r)).ToList();
                var shim = this.GetShim(RelativeFileName);
                if (shim != null)
                {
                    Dependencies.AddRange(shim.Dependencies.Select(r => this.GetModulePath(r.Dependency)));
                }

                Dependencies = Dependencies.Distinct().ToList();

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

                var transformations = this.GetTransformations(result, flattenedResults);
                var text = OriginalString;
                transformations.ExecuteAll(ref text);
                ProcessedString = text;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error while processing {0}: {1}", RelativeFileName, ex.Message), ex);
            }
            
        }

        private TransformationCollection GetTransformations(VisitorResult result, List<RequireCall> flattened)
        {
            var trans = new TransformationCollection();

            if (!result.RequireCalls.Any())
            {
                var shim = GetShim(RelativeFileName);
                var deps = new List<string>();
                if (shim != null)
                {
                    deps = shim.Dependencies.Select(r => r.Dependency).ToList();
                }

                trans.Add(ShimFileTransformation.Create(this.CheckForConfigPath(RelativeFileName.ToModuleName()), deps));
            }
            else
            {
                foreach (var reqCall in result.RequireCalls.Where(r => r.DependencyArrayNode != null))    
                {
                    trans.Add(NormalizeDepsTransformation.Create(reqCall));
                }

                // if there are no define calls but there is at least one require module call, transform that into a define call
                if (!result.RequireCalls.Where(r => r.Type == RequireCallType.Define).Any())
                {
                    if (result.RequireCalls.Where(r => r.IsModule).Any())
                    {
                        var call = result.RequireCalls.Where(r => r.IsModule).FirstOrDefault();
                        trans.Add(ToDefineTransformation.Create(call));
                        trans.Add(AddIdentifierTransformation.Create(call,this.CheckForConfigPath(RelativeFileName.ToModuleName())));
                    }
                }
                else
                {
                    var defineCall = result.RequireCalls.Where(r => r.Type == RequireCallType.Define).FirstOrDefault();
                    if (string.IsNullOrEmpty(defineCall.Id))
                    {
                        trans.Add(AddIdentifierTransformation.Create(defineCall, this.CheckForConfigPath(RelativeFileName.ToModuleName())));
                    }

                    if (defineCall.DependencyArrayNode == null)
                    {
                        trans.Add(AddEmptyDepsArrayTransformation.Create(defineCall));
                    }
                }
            }

            return trans;
        }

        private ShimEntry GetShim(string relativeFileName)
        {
            return configuration.Shim.ShimEntries.Where(r => r.For.ToLower() == relativeFileName.ToModuleName().ToLower()
                                                                    || r.For.ToLower() == this.CheckForConfigPath(relativeFileName.ToModuleName()).ToLower())
                                                        .FirstOrDefault();
        }

        private string CheckForConfigPath(string name)
        {
            var result = name;
            var pathEl = configuration.Paths.PathList.Where(r => r.Value.ToLower() == name.ToLower()).FirstOrDefault();
            if (pathEl != null)
            {
                result = pathEl.Key;
            }

            return result;
        }

        private string GetModulePath(string name)
        {
            var result = name;
            var pathEl = configuration.Paths.PathList.Where(r => r.Key.ToLower() == name.ToLower()).FirstOrDefault();
            if (pathEl != null)
            {
                result = pathEl.Value;
            }

            return result;
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
                // we could also be at the last character of the file when that isn't a newline
                if ((currChar == '\r' || currChar == '\n') || i == scriptText.Length - 1)
                {
                    if (currChar != '\r' && currChar != '\n')
                    {
                        currentLineBuilder.Append(scriptText[i]); 
                    }

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
