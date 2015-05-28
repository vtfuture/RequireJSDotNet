// RequireJS.NET
// Copyright VeriTech.io
// http://veritech.io
// Dual licensed under the MIT and GPL licenses:
// http://www.opensource.org/licenses/mit-license.php
// http://www.gnu.org/licenses/gpl.html

using System;
using System.Collections.Generic;
using System.Linq;

using Jint;
using Jint.Parser;
using Jint.Parser.Ast;

using RequireJsNet.Compressor.Models;

namespace RequireJsNet.Compressor.Parsing
{
    /// <summary>
    /// A specialized visitor class that will extract require() and define() calls
    /// TODO: could make this an actual visitor and implement the require() specific code in another class
    /// </summary>
    internal class RequireVisitor
    {
        private VisitorResult result = new VisitorResult();

        private Program program;

        private List<NodeWithChildren> visitedNodes = new List<NodeWithChildren>(); 

        private List<Location> nodesToSkip = new List<Location>();

        private string relativeFileName;

        public VisitorResult Visit(Program program, string relativeFileName)
        {
            if (program == null)
            {
                return null;
            }
            this.relativeFileName = relativeFileName;
            this.program = program;

            this.VisitNodeEnumerator(program.Body, null, null);
            return result;
        }

        private void VisitStatement(Statement statement, RequireCall parentCall, NodeWithChildren parentNode)
        {
            if (statement == null || this.ShouldSkipNode(statement))
            {
                return;
            }

            var currentNode = new NodeWithChildren { Node = statement };
            if (parentNode != null)
            {
                currentNode.Parent = parentNode;
                parentNode.Children.Add(currentNode);
            }
            else
            {
                visitedNodes.Add(currentNode);
            }

            switch (statement.Type)
            {
                case SyntaxNodes.BlockStatement:
                    var block = statement.As<BlockStatement>();
                    VisitNodeEnumerator(block.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.DoWhileStatement:
                    var doWhile = statement.As<DoWhileStatement>();
                    VisitStatement(doWhile.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.ExpressionStatement:
                    var expression = statement.As<ExpressionStatement>();
                    VisitExpression(expression.Expression, parentCall, currentNode);
                    break;
                case SyntaxNodes.ForInStatement:
                    var forIn = statement.As<ForInStatement>();
                    VisitStatement(forIn.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.ForStatement:
                    var forStatement = statement.As<ForStatement>();
                    VisitStatement(forStatement.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.FunctionDeclaration:
                    var funcDeclaration = statement.As<FunctionDeclaration>();
                    VisitStatement(funcDeclaration.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.IfStatement:
                    var ifStatement = statement.As<IfStatement>();
                    VisitStatement(ifStatement.Alternate, parentCall, currentNode);
                    VisitStatement(ifStatement.Consequent, parentCall, currentNode);
                    break;
                case SyntaxNodes.ReturnStatement:
                    var returnStatement = statement.As<ReturnStatement>();
                    VisitExpression(returnStatement.Argument, parentCall, currentNode);
                    break;
                case SyntaxNodes.TryStatement:
                    var tryStatement = statement.As<TryStatement>();
                    VisitStatement(tryStatement.Block, parentCall, currentNode);
                    break;
                case SyntaxNodes.CatchClause:
                    var catchClause = statement.As<CatchClause>();
                    VisitStatement(catchClause.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.VariableDeclaration:
                    var varDeclaration = statement.As<VariableDeclaration>();
                    VisitNodeEnumerator(varDeclaration.Declarations, parentCall, currentNode);
                    break;
                case SyntaxNodes.WhileStatement:
                    var whileStatement = statement.As<WhileStatement>();
                    VisitStatement(whileStatement.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.WithStatement:
                    var withStatement = statement.As<WithStatement>();
                    VisitStatement(withStatement.Body, parentCall, currentNode);
                    break;
                default:
                    break;
            }
        }

        private void VisitNodeEnumerator(IEnumerable<SyntaxNode> nodes, RequireCall parentCall, NodeWithChildren parentNode)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                if (this.ShouldSkipNode(node))
                {
                    return;
                }

                var currentNode = new NodeWithChildren { Node = node };
                if (parentNode != null)
                {
                    currentNode.Parent = parentNode;
                    parentNode.Children.Add(currentNode);
                }
                else
                {
                    visitedNodes.Add(currentNode);
                }

                if (node is Statement)
                {
                    VisitStatement(node as Statement, parentCall, currentNode);
                }
                else if (node is Expression)
                {
                    VisitExpression(node as Expression, parentCall, currentNode);
                }
                else if (node is SwitchCase)
                {
                    VisitSwitchCase(node as SwitchCase, parentCall, currentNode);
                }
            }
        }

        private void VisitExpression(Expression node, RequireCall parentCall, NodeWithChildren parentNode)
        {
            if (node == null || this.ShouldSkipNode(node))
            {
                return;
            }

            var currentNode = new NodeWithChildren { Node = node };
            if (parentNode != null)
            {
                currentNode.Parent = parentNode;
                parentNode.Children.Add(currentNode);
            }
            else
            {
                visitedNodes.Add(currentNode);
            }

            switch (node.Type)
            {
                case SyntaxNodes.ArrayExpression:
                    var arrExpression = node.As<ArrayExpression>();
                    VisitNodeEnumerator(arrExpression.Elements, parentCall, currentNode);
                    break;
                case SyntaxNodes.AssignmentExpression:
                    var assignment = node.As<AssignmentExpression>();
                    VisitExpression(assignment.Right, parentCall, currentNode);
                    break;
                case SyntaxNodes.BinaryExpression:
                    var binaryExpression = node.As<BinaryExpression>();
                    VisitExpression(binaryExpression.Left, parentCall, currentNode);
                    VisitExpression(binaryExpression.Right, parentCall, currentNode);
                    break;
                case SyntaxNodes.CallExpression:
                    // TOOD: do stuff with this
                    var callExpression = node.As<CallExpression>();
                    var callee = callExpression.Callee;
                    if (callee.Type == SyntaxNodes.Identifier)
                    {
                        var calleeIdentifier = callee.As<Identifier>();

                        if (calleeIdentifier.Name == "require")
                        {
                            ProcessRequireCall(ref parentCall, callExpression, currentNode);
                        }

                        if (calleeIdentifier.Name == "define")
                        {
                            ProcessDefineCall(ref parentCall, callExpression, currentNode);
                        }
                    }
                    else
                    {
                        this.VisitExpression(callee, parentCall, currentNode);
                        VisitNodeEnumerator(callExpression.Arguments, parentCall, currentNode);
                    }

                    break;
                case SyntaxNodes.ConditionalExpression:
                    var conditionalExpression = node.As<ConditionalExpression>();
                    VisitExpression(conditionalExpression.Consequent, parentCall, currentNode);
                    VisitExpression(conditionalExpression.Alternate, parentCall, currentNode);
                    break;
                case SyntaxNodes.FunctionExpression:
                    var functionExpression = node.As<FunctionExpression>();
                    VisitStatement(functionExpression.Body, parentCall, currentNode);
                    break;
                case SyntaxNodes.LogicalExpression:
                    var logicalExpression = node.As<LogicalExpression>();
                    VisitExpression(logicalExpression.Left, parentCall, currentNode);
                    VisitExpression(logicalExpression.Right, parentCall, currentNode);
                    break;
                case SyntaxNodes.NewExpression:
                    var newExpression = node.As<NewExpression>();
                    VisitNodeEnumerator(newExpression.Arguments, parentCall, currentNode);
                    VisitExpression(newExpression.Callee, parentCall, currentNode);
                    break;
                case SyntaxNodes.ObjectExpression:
                    var objectExpression = node.As<ObjectExpression>();
                    VisitNodeEnumerator(objectExpression.Properties, parentCall, currentNode);
                    break;
                case SyntaxNodes.Property:
                    var property = node.As<Property>();
                    VisitExpression(property.Value, parentCall, currentNode);
                    break;
                case SyntaxNodes.SequenceExpression:
                    var sequence = node.As<SequenceExpression>();
                    VisitNodeEnumerator(sequence.Expressions, parentCall, currentNode);
                    break;
                case SyntaxNodes.UnaryExpression:
                    var unary = node.As<UnaryExpression>();
                    VisitExpression(unary.Argument, parentCall, currentNode);
                    break;
                case SyntaxNodes.VariableDeclarator:
                    var variableDeclarator = node.As<VariableDeclarator>();
                    VisitExpression(variableDeclarator.Init, parentCall, currentNode);
                    break;
                default:
                    break;
            }
        }

        private void VisitSwitchCase(SwitchCase switchCase, RequireCall parentCall, NodeWithChildren parentNode)
        {
            if (this.ShouldSkipNode(switchCase))
            {
                return;
            }

            var currentNode = new NodeWithChildren { Node = switchCase };
            if (parentNode != null)
            {
                currentNode.Parent = parentNode;
                parentNode.Children.Add(currentNode);
            }
            else
            {
                visitedNodes.Add(currentNode);
            }

            foreach (var statement in switchCase.Consequent)
            {
                VisitStatement(statement, parentCall, currentNode);
            }
        }

        private void ProcessRequireCall(ref RequireCall parentCall, CallExpression callExpression, NodeWithChildren parentNode)
        {
            var argCount = callExpression.Arguments.Count();
            if (argCount < 1 || argCount > 2)
            {
                throw new Exception("Invalid number of arguments for require() call " + relativeFileName);
            }

            var requireCall = new RequireCall
                                  {
                                      Type = RequireCallType.Require,
                                      ParentNode = parentNode
                                  };

            if (parentCall != null)
            {
                parentCall.Children.Add(requireCall);
            }
            else
            {
                result.RequireCalls.Add(requireCall);
            }

            parentCall = requireCall;

            var firstArg = callExpression.Arguments.First();
            var secondArg = callExpression.Arguments.Last();

            if (argCount == 1)
            {
                if (firstArg is Literal)
                {
                    var singleDep = firstArg.As<Literal>();
                    requireCall.SingleDependencyNode = singleDep;
                    requireCall.Dependencies.Add(singleDep.Value.ToString());
                }
                if (firstArg is ArrayExpression)
                {
                    var deps = this.ProcessDependencyArray(firstArg, requireCall);
                    requireCall.Dependencies.AddRange(deps);
                }
                else
                {
                    throw new Exception("Could not read argument for require() call " + relativeFileName);
                }
            }
            else if (argCount == 2)
            {
                requireCall.IsModule = true;
                var deps = this.ProcessDependencyArray(firstArg, requireCall);
                requireCall.Dependencies.AddRange(deps);

                this.ProcessModuleDefinition(secondArg, parentCall, parentNode);
            }
        }

        private void ProcessDefineCall(ref RequireCall parentCall, CallExpression callExpression, NodeWithChildren parentNode)
        {
            var argCount = callExpression.Arguments.Count();
            if (argCount < 1 || argCount > 3)
            {
                throw new Exception("Invalid number of arguments for define() call " + relativeFileName);
            }

            var defineCall = new RequireCall
                                 {
                                     Type = RequireCallType.Define,
                                     IsModule = true,
                                     ParentNode = parentNode
                                 };

            if (parentCall != null)
            {
                parentCall.Children.Add(defineCall);
            }
            else
            {
                result.RequireCalls.Add(defineCall);
            }

            parentCall = defineCall;

            Expression moduleDefinition = null;
            Expression depsArray = null;

            // define('name', [deps], function () {})
            if (argCount == 3)
            {
                depsArray = callExpression.Arguments.ElementAt(1);
                moduleDefinition = callExpression.Arguments.ElementAt(2);

                var identifierLiteral = callExpression.Arguments.ElementAt(0).As<Literal>();
                if (identifierLiteral == null)
                {
                    throw new Exception("The first argument in a define call with 3 arguments was not a string literal." + relativeFileName);
                }

                parentCall.ModuleIdentifierNode = identifierLiteral;
                defineCall.Id = identifierLiteral.Value.ToString();
            }

            // define([deps], function () {})
            if (argCount == 2)
            {
                depsArray = callExpression.Arguments.ElementAt(0);
                moduleDefinition = callExpression.Arguments.ElementAt(1);
            }

            // define(function () {})
            if (argCount == 1)
            {
                moduleDefinition = callExpression.Arguments.ElementAt(0);
            }

            if (depsArray != null)
            {
                defineCall.Dependencies.AddRange(this.ProcessDependencyArray(depsArray, parentCall));    
            }
            
            ProcessModuleDefinition(moduleDefinition, parentCall, parentNode);
        }

        private IEnumerable<string> ProcessDependencyArray(Expression depsNode, RequireCall parentCall)
        {
            var depsArray = depsNode.As<ArrayExpression>();
            if (depsArray == null)
            {
                yield break;

                // throw new Exception("Dependency array node was not an ArrayExpression " + relativeFileName);    
            }

            parentCall.DependencyArrayNode = depsArray;

            foreach (var expression in depsArray.Elements)
            {
                
                if (expression is Literal)
                {
                    var val = expression.As<Literal>();    
                    yield return val.Value.ToString();
                }

                // TODO: decide if we want to ignore, throw exception or add basic support for string concatenation
                // An implementation where the user could define a value for a variable name in case of string concatenation
                // (or a set of values, and we would all of the possible values in the dependency array)
                //// throw new Exception("One of the elements in a require() dependency array was not a string literal");
               
            }
        }

        private void ProcessModuleDefinition(Expression moduleNode, RequireCall parentCall, NodeWithChildren parentNode)
        {
            // hardcoded check for the factory pattern that cheks if we have amd support
            // this will happen when the module node is an identifier.
            // We'll go up the tree until we find the first enclosing function definition. If that 
            // function has an argument with the same identifier as the module and the function is self-calling with a function 
            // as its parameter, we'll process that function as a child of this requireCall
            if (moduleNode is Identifier)
            {
                var moduleIdentifier = moduleNode.As<Identifier>();

                // backtrack to the first callExpression, skipping the one that we're in
                var skipped = 0;
                var currentParent = parentNode;
                while (currentParent != null)
                {
                    if (currentParent.Node is CallExpression)
                    {
                        if (skipped > 0)
                        {
                            break;    
                        }

                        skipped++;
                    }

                    currentParent = currentParent.Parent;
                }

                if (currentParent == null)
                {
                    return;
                }

                var containingCall = currentParent.Node.As<CallExpression>();
                if (containingCall.Arguments.Count() != 1)
                {
                    return;
                }

                // check if our containing function has one argument with the same name as the factory identifier we've received
                var containingCallee = containingCall.Callee.As<FunctionExpression>();
                if (containingCallee.Parameters.Count() != 1)
                {
                    return;
                }

                var calleeParam = containingCallee.Parameters.ElementAt(0).As<Identifier>();
                if (calleeParam == null)
                {
                    return;
                }

                if (calleeParam.Name != moduleIdentifier.Name)
                {
                    return;
                }

                var argumentFunction = containingCall.Arguments.ElementAt(0).As<FunctionExpression>();
                if (argumentFunction == null)
                {
                    return;
                }

                parentCall.ModuleDefinitionNode = argumentFunction;
                this.VisitExpression(argumentFunction, parentCall, currentParent);
                this.nodesToSkip.Add(argumentFunction.Location);
            }
            else
            {
                this.VisitExpression(moduleNode, parentCall, parentNode);    
            }
        }

        private bool ShouldSkipNode(SyntaxNode node)
        {
            return nodesToSkip.Any(r => r.Start.Line == node.Location.Start.Line 
                                        && r.Start.Column == node.Location.Start.Column
                                        && r.End.Line == node.Location.End.Line 
                                        && r.End.Column == node.Location.End.Column);
        }
    }
}
