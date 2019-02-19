namespace SimpleAuth.Tests.Helpers
{
    using Exceptions;
    using Shared;
    using SimpleAuth.Api.Authorization;
    using System.Collections.Generic;
    using SimpleAuth.Extensions;
    using SimpleAuth.Shared.Errors;
    using Xunit;

    public sealed class AuthorizationFlowHelperFixture
    {
        [Fact]
        public void When_Passing_No_Response_Type_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            ICollection<string> collection = null;
            var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => collection.GetAuthorizationFlow(state));
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationFlowIsNotSupported, exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public void When_Passing_Empty_List_Of_Response_Types_Then_Exception_Is_Thrown()
        {
            const string state = "state";

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => new List<string>().GetAuthorizationFlow(state));
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationFlowIsNotSupported, exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public void When_Passing_Code_Then_Authorization_Code_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = new[] { ResponseTypeNames.Code }.GetAuthorizationFlow(state);

            Assert.Equal(AuthorizationFlow.AuthorizationCodeFlow, result);
        }

        [Fact]
        public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = new List<string> { ResponseTypeNames.IdToken }.GetAuthorizationFlow(state);

            Assert.Equal(AuthorizationFlow.ImplicitFlow, result);
        }

        [Fact]
        public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = new List<string> { ResponseTypeNames.IdToken, ResponseTypeNames.Code }.GetAuthorizationFlow(state);

            Assert.Equal(AuthorizationFlow.HybridFlow, result);
        }
    }
}
