namespace Reimers.Scim.Query
{
    using Antlr4.Runtime.Tree;
    using System.Linq.Expressions;

    internal interface IScimFilterVisitor
    {
        LambdaExpression VisitExpression(IParseTree tree);
    }
}