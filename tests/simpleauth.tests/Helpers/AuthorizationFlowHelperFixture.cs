namespace SimpleAuth.Tests.Helpers
{
    using Errors;
    using Exceptions;
    using Shared;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Helpers;
    using System.Collections.Generic;
    using Xunit;

    public sealed class AuthorizationFlowHelperFixture
    {
        private IAuthorizationFlowHelper _authorizationFlowHelper;

        public AuthorizationFlowHelperFixture()
        {
            InitializeFakeObjects();
        }

        [Fact]
        public void When_Passing_No_Response_Type_Then_Exception_Is_Thrown()
        {
            const string state = "state";

            var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(
                    () => _authorizationFlowHelper.GetAuthorizationFlow(null, state));
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.Equal(ErrorDescriptions.TheAuthorizationFlowIsNotSupported, exception.Message);
            Assert.Equal(state, exception.State);
        }

        [Fact]
        public void When_Passing_Empty_List_Of_Response_Types_Then_Exception_Is_Thrown()
        {
            const string state = "state";

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(
                () => _authorizationFlowHelper.GetAuthorizationFlow(new List<string>(), state));
            Assert.Equal(ErrorCodes.InvalidRequestCode, exception.Code);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Passing_Code_Then_Authorization_Code_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = _authorizationFlowHelper.GetAuthorizationFlow(new[] { ResponseTypeNames.Code }, state);

            Assert.True(result == AuthorizationFlow.AuthorizationCodeFlow);
        }

        [Fact]
        public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<string> { ResponseTypeNames.IdToken },
                state);

            Assert.True(result == AuthorizationFlow.ImplicitFlow);
        }

        [Fact]
        public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
        {
            const string state = "state";

            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<string> { ResponseTypeNames.IdToken, ResponseTypeNames.Code },
                state);

            Assert.Equal(AuthorizationFlow.HybridFlow, result);
        }

        private void InitializeFakeObjects()
        {
            _authorizationFlowHelper = new AuthorizationFlowHelper();
        }
    }
}
