using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequireJsNet.Compressor.Parsing
{
    using Jint;
    using Jint.Parser.Ast;

    public class RequireVisitor
    {
        static void VisitStatement(Statement statement)
        {
            if (statement == null)
            {
                return;
            }

            switch (statement.Type)
            {
                case SyntaxNodes.BlockStatement:
                    var block = statement.As<BlockStatement>();
                    VisitNodeEnumerator(block.Body);
                    break;
                case SyntaxNodes.DoWhileStatement:
                    var doWhile = statement.As<DoWhileStatement>();
                    VisitStatement(doWhile.Body);
                    break;
                case SyntaxNodes.ExpressionStatement:
                    var expression = statement.As<ExpressionStatement>();
                    VisitExpression(expression.Expression);
                    break;
                case SyntaxNodes.ForInStatement:
                    var forIn = statement.As<ForInStatement>();
                    VisitStatement(forIn.Body);
                    break;
                case SyntaxNodes.ForStatement:
                    var forStatement = statement.As<ForStatement>();
                    VisitStatement(forStatement.Body);
                    break;
                case SyntaxNodes.FunctionDeclaration:
                    var funcDeclaration = statement.As<FunctionDeclaration>();
                    VisitStatement(funcDeclaration.Body);
                    break;
                case SyntaxNodes.IfStatement:
                    var ifStatement = statement.As<IfStatement>();
                    VisitStatement(ifStatement.Alternate);
                    VisitStatement(ifStatement.Consequent);
                    break;
                case SyntaxNodes.ReturnStatement:
                    var returnStatement = statement.As<ReturnStatement>();
                    VisitExpression(returnStatement.Argument);
                    break;
                case SyntaxNodes.TryStatement:
                    var tryStatement = statement.As<TryStatement>();
                    VisitStatement(tryStatement.Block);
                    break;
                case SyntaxNodes.CatchClause:
                    var catchClause = statement.As<CatchClause>();
                    VisitStatement(catchClause.Body);
                    break;
                case SyntaxNodes.VariableDeclaration:
                    var varDeclaration = statement.As<VariableDeclaration>();
                    VisitNodeEnumerator(varDeclaration.Declarations);
                    break;
                case SyntaxNodes.WhileStatement:
                    var whileStatement = statement.As<WhileStatement>();
                    VisitStatement(whileStatement.Body);
                    break;
                case SyntaxNodes.WithStatement:
                    var withStatement = statement.As<WithStatement>();
                    VisitStatement(withStatement.Body);
                    break;
                default:
                    break;
            }
        }

        static void VisitNodeEnumerator(IEnumerable<SyntaxNode> nodes)
        {
            if (nodes == null)
            {
                return;
            }

            foreach (var node in nodes)
            {
                if (node is Statement)
                {
                    VisitStatement(node as Statement);
                }
                else if (node is Expression)
                {
                    VisitExpression(node as Expression);
                }
                else if (node is SwitchCase)
                {
                    VisitSwitchCase(node as SwitchCase);
                }
            }
        }

        static void VisitExpression(Expression node)
        {
            if (node == null)
            {
                return;
            }

            switch (node.Type)
            {
                case SyntaxNodes.ArrayExpression:
                    var arrExpression = node.As<ArrayExpression>();
                    VisitNodeEnumerator(arrExpression.Elements);
                    break;
                case SyntaxNodes.AssignmentExpression:
                    var assignment = node.As<AssignmentExpression>();
                    VisitExpression(assignment.Right);
                    break;
                case SyntaxNodes.BinaryExpression:
                    var binaryExpression = node.As<BinaryExpression>();
                    VisitExpression(binaryExpression.Left);
                    VisitExpression(binaryExpression.Right);
                    break;
                case SyntaxNodes.CallExpression:
                    // TOOD: do stuff with this
                    var callExpression = node.As<CallExpression>();
                    var callee = callExpression.Callee;
                    if (callee.Type == SyntaxNodes.Identifier
                        && (callee.As<Identifier>().Name == "require"
                        || callee.As<Identifier>().Name == "define"))
                    {
                        (callee.As<Identifier>()).Name = "require";
                        var bdgd = 5;
                    }

                    VisitNodeEnumerator(callExpression.Arguments);
                    break;
                case SyntaxNodes.ConditionalExpression:
                    var conditionalExpression = node.As<ConditionalExpression>();
                    VisitExpression(conditionalExpression.Consequent);
                    VisitExpression(conditionalExpression.Alternate);
                    break;
                case SyntaxNodes.FunctionExpression:
                    var functionExpression = node.As<FunctionExpression>();
                    VisitStatement(functionExpression.Body);
                    break;
                case SyntaxNodes.LogicalExpression:
                    var logicalExpression = node.As<LogicalExpression>();
                    VisitExpression(logicalExpression.Left);
                    VisitExpression(logicalExpression.Right);
                    break;
                case SyntaxNodes.NewExpression:
                    var newExpression = node.As<NewExpression>();
                    VisitNodeEnumerator(newExpression.Arguments);
                    VisitExpression(newExpression.Callee);
                    break;
                case SyntaxNodes.ObjectExpression:
                    var objectExpression = node.As<ObjectExpression>();
                    VisitNodeEnumerator(objectExpression.Properties);
                    break;
                case SyntaxNodes.Property:
                    var property = node.As<Property>();
                    VisitExpression(property.Value);
                    break;
                case SyntaxNodes.SequenceExpression:
                    var sequence = node.As<SequenceExpression>();
                    VisitNodeEnumerator(sequence.Expressions);
                    break;
                case SyntaxNodes.UnaryExpression:
                    var unary = node.As<UnaryExpression>();
                    VisitExpression(unary.Argument);
                    break;
                case SyntaxNodes.VariableDeclarator:
                    var variableDeclarator = node.As<VariableDeclarator>();
                    VisitExpression(variableDeclarator.Init);
                    break;
                default:
                    break;
            }
        }

        static void VisitSwitchCase(SwitchCase switchCase)
        {
            foreach (var statement in switchCase.Consequent)
            {
                VisitStatement(statement);
            }
        }
    }
}
