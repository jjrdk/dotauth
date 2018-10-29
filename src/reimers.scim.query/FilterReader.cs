namespace reimers.scim.query.tests
{
    using Antlr4.Runtime;
    using Reimers.Scim.Query;
    using System.IO;
    using System.Linq.Expressions;

    public class FilterReader<T>
    {
        public LambdaExpression Read(string filter)
        {
            using (var reader = new StringReader(filter))
            {
                return CreateLambdaExpression(reader);
            }
        }

        public LambdaExpression Read(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return CreateLambdaExpression(reader);
            }
        }

        private static LambdaExpression CreateLambdaExpression(TextReader reader)
        {
            var visitor = new ScimFilterVisitor<T>();
            var tokenStream =
                new UnbufferedTokenStream(new ScimFilterLexer(new AntlrInputStream(reader)));
            var parser = new ScimFilterParser(tokenStream);
            var context = parser.parse();
            var expression = visitor.Visit(context);
            return expression;
        }
    }
}
