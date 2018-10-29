namespace reimers.scim.query.tests
{
    using SimpleIdentityServer.Core.Common.DTOs;
    using Xunit;
    using Xunit.Abstractions;

    public class QueryExpressionTests
    {
        private readonly ITestOutputHelper _output;

        public QueryExpressionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData("UserName pr", "Param_0 => IsPresent(Invoke(Param_1 => Param_1.UserName, Param_0))")]
        public void CanParseQuery(string query, string expectedExpression)
        {
            var reader = new FilterReader<ScimUser>();
            var expression = reader.Read(query);
            
            _output.WriteLine(expression.ToString());

            Assert.Equal(expectedExpression, expression.ToString());
        }
    }
}