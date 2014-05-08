using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Parsing
{
    using Jint;
    using Jint.Parser.Ast;
    
    /// <summary>
    /// A specialized visitor class that will extract require() and define() calls
    /// TODO: could make this an actual visitor and implement the require() specific code in another class
    /// </summary>
    internal class RequireVisitor
    {
        private VisitorResult result = new VisitorResult();

        public VisitorResult Visit(Program program)
        {
            if (program == null)
            {
                return null;
            }

            this.VisitNodeEnumerator(program.Body, null);
            return result;
        }

        private void VisitStatement(Statement statement, RequireCall parentCall)
        {
            if (statement == null)
            {
                return;
            }

            switch (statement.Type)
            {
                case SyntaxNodes.BlockStatement:
                    var block = statement.As<BlockStatement>();
                    VisitNodeEnumerator(block.Body, parentCall);
                    break;
                case SyntaxNodes.DoWhileStatement:
                    var doWhile = statement.As<DoWhileStatement>();
                    VisitStatement(doWhile.Body, parentCall);
                    break;
                case SyntaxNodes.ExpressionStatement:
                    var expression = statement.As<ExpressionStatement>();
                    VisitExpression(expression.Expression, parentCall);
                    break;
                case SyntaxNodes.ForInStatement:
                    var forIn = statement.As<ForInStatement>();
                    VisitStatement(forIn.Body, parentCall);
                    break;
                case SyntaxNodes.ForStatement:
                    var forStatement = statement.As<ForStatement>();
                    VisitStatement(forStatement.Body, parentCall);
                    break;
                case SyntaxNodes.FunctionDeclaration:
                    var funcDeclaration = statement.As<FunctionDeclaration>();
                    VisitStatement(funcDeclaration.Body, parentCall);
                    break;
                case SyntaxNodes.IfStatement:
                    var ifStatement = statement.As<IfStatement>();
                    VisitStatement(ifStatement.Alternate, parentCall);
                    VisitStatement(ifStatement.Consequent, parentCall);
                    break;
                case SyntaxNodes.ReturnStatement:
                    var returnStatement = statement.As<ReturnStatement>();
                    VisitExpression(returnStatement.Argument, parentCall);
                    break;
                case SyntaxNodes.TryStatement:
                    var tryStatement = statement.As<TryStatement>();
                    VisitStatement(tryStatement.Block, parentCall);
                    break;
                case SyntaxNodes.CatchClause:
                    var catchClause = statement.As<CatchClause>();
                    VisitStatement(catchClause.Body, parentCall);
                    break;
                case SyntaxNodes.VariableDeclaration:
                    var varDeclaration = statement.As<VariableDeclaration>();
                    VisitNodeEnumerator(varDeclaration.Declarations, parentCall);
                    break;
                case SyntaxNodes.WhileStatement:
                    var whileStatement = statement.As<WhileStatement>();
                    VisitStatement(whileStatement.Body, parentCall);
                    break;
                case SyntaxNodes.WithStatement:
                    var withStatement = statement.As<WithStatement>();
                    VisitStatement(withStatement.Body, parentCall);
                    break;
                default:
                    break;
            }
        }

        private void VisitNodeEnumerator(IEnumerable<SyntaxNode> nodes, RequireCall parentCall)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                if (node is Statement)
                {
                    VisitStatement(node as Statement, parentCall);
                }
                else if (node is Expression)
                {
                    VisitExpression(node as Expression, parentCall);
                }
                else if (node is SwitchCase)
                {
                    VisitSwitchCase(node as SwitchCase, parentCall);
                }
            }
        }

        private void VisitExpression(Expression node, RequireCall parentCall)
        {
            if (node == null)
            {
                return;
            }

            switch (node.Type)
            {
                case SyntaxNodes.ArrayExpression:
                    var arrExpression = node.As<ArrayExpression>();
                    VisitNodeEnumerator(arrExpression.Elements, parentCall);
                    break;
                case SyntaxNodes.AssignmentExpression:
                    var assignment = node.As<AssignmentExpression>();
                    VisitExpression(assignment.Right, parentCall);
                    break;
                case SyntaxNodes.BinaryExpression:
                    var binaryExpression = node.As<BinaryExpression>();
                    VisitExpression(binaryExpression.Left, parentCall);
                    VisitExpression(binaryExpression.Right, parentCall);
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
                            var argCount = callExpression.Arguments.Count();
                            if (argCount < 1 || argCount > 2)
                            {
                                throw new Exception("Invalid number of arguments for require() call");
                            }

                            var requireCall = new RequireCall();

                            if (parentCall != null)
                            {
                                parentCall.Children.Add(requireCall);
                            }
                            else
                            {
                                result.RequireCalls.Add(requireCall);
                            }

                            var firstArg = callExpression.Arguments.First();
                            var secondArg = callExpression.Arguments.Last();

                            parentCall = requireCall;

                            if (argCount == 1)
                            {
                                var singleDep = firstArg.As<Literal>();
                                if (singleDep == null)
                                {
                                    throw new Exception("Could not read argument for require() call");
                                }

                                requireCall.Dependencies.Add(singleDep.Value.ToString());
                            }
                            else if (argCount == 2)
                            {
                                var depArr = firstArg.As<ArrayExpression>();
                                if (depArr == null)
                                {
                                    throw new Exception("Could not read dependency array for require() call");
                                }

                                foreach (var expression in depArr.Elements)
                                {
                                    var val = expression.As<Literal>();
                                    if (val == null)
                                    {
                                        throw new Exception("One of the elements in a require() dependency array was not a string literal");
                                    }

                                    requireCall.Dependencies.Add(val.Value.ToString());
                                }

                                this.VisitExpression(secondArg, parentCall);
                            }
                        }

                        if (calleeIdentifier.Name == "define")
                        {
                            
                        }
                    }
                    else
                    {
                        this.VisitExpression(callee, parentCall);
                        VisitNodeEnumerator(callExpression.Arguments, parentCall);
                    }

                    break;
                case SyntaxNodes.ConditionalExpression:
                    var conditionalExpression = node.As<ConditionalExpression>();
                    VisitExpression(conditionalExpression.Consequent, parentCall);
                    VisitExpression(conditionalExpression.Alternate, parentCall);
                    break;
                case SyntaxNodes.FunctionExpression:
                    var functionExpression = node.As<FunctionExpression>();
                    VisitStatement(functionExpression.Body, parentCall);
                    break;
                case SyntaxNodes.LogicalExpression:
                    var logicalExpression = node.As<LogicalExpression>();
                    VisitExpression(logicalExpression.Left, parentCall);
                    VisitExpression(logicalExpression.Right, parentCall);
                    break;
                case SyntaxNodes.NewExpression:
                    var newExpression = node.As<NewExpression>();
                    VisitNodeEnumerator(newExpression.Arguments, parentCall);
                    VisitExpression(newExpression.Callee, parentCall);
                    break;
                case SyntaxNodes.ObjectExpression:
                    var objectExpression = node.As<ObjectExpression>();
                    VisitNodeEnumerator(objectExpression.Properties, parentCall);
                    break;
                case SyntaxNodes.Property:
                    var property = node.As<Property>();
                    VisitExpression(property.Value, parentCall);
                    break;
                case SyntaxNodes.SequenceExpression:
                    var sequence = node.As<SequenceExpression>();
                    VisitNodeEnumerator(sequence.Expressions, parentCall);
                    break;
                case SyntaxNodes.UnaryExpression:
                    var unary = node.As<UnaryExpression>();
                    VisitExpression(unary.Argument, parentCall);
                    break;
                case SyntaxNodes.VariableDeclarator:
                    var variableDeclarator = node.As<VariableDeclarator>();
                    VisitExpression(variableDeclarator.Init, parentCall);
                    break;
                default:
                    break;
            }
        }

        private void VisitSwitchCase(SwitchCase switchCase, RequireCall parentCall)
        {
            foreach (var statement in switchCase.Consequent)
            {
                VisitStatement(statement, parentCall);
            }
        }
    }
}
