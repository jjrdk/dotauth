namespace SimpleAuth.Tests.Helpers
{
    using Errors;
    using Exceptions;
    using Shared.Requests;
    using SimpleAuth.Api.Authorization;
    using SimpleAuth.Helpers;
    using System.Collections.Generic;
    using Shared;
    using Xunit;

    public sealed class AuthorizationFlowHelperFixture
    {
        private IAuthorizationFlowHelper _authorizationFlowHelper;

        [Fact]
        public void When_Passing_No_Response_Type_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            InitializeFakeObjects();

            var exception =
                Assert.Throws<SimpleAuthExceptionWithState>(() =>
                    _authorizationFlowHelper.GetAuthorizationFlow(null, state));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Passing_Empty_List_Of_Response_Types_Then_Exception_Is_Thrown()
        {
            const string state = "state";
            InitializeFakeObjects();

            var exception = Assert.Throws<SimpleAuthExceptionWithState>(() =>
                _authorizationFlowHelper.GetAuthorizationFlow(
                    new List<string>(),
                    state));
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheAuthorizationFlowIsNotSupported);
            Assert.True(exception.State == state);
        }

        [Fact]
        public void When_Passing_Code_Then_Authorization_Code_Flow_Should_Be_Returned()
        {
            const string state = "state";
            InitializeFakeObjects();

            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new[] {ResponseTypeNames.Code},
                state);

            Assert.True(result == AuthorizationFlow.AuthorizationCodeFlow);
        }

        [Fact]
        public void When_Passing_Id_Token_Then_Implicit_Flow_Should_Be_Returned()
        {
            const string state = "state";
            InitializeFakeObjects();

            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<string> {ResponseTypeNames.IdToken},
                state);

            Assert.True(result == AuthorizationFlow.ImplicitFlow);
        }

        [Fact]
        public void When_Passing_Code_And_Id_Token_Then_Hybrid_Flow_Should_Be_Returned()
        {
            const string state = "state";
            InitializeFakeObjects();

            var result = _authorizationFlowHelper.GetAuthorizationFlow(
                new List<string> {ResponseTypeNames.IdToken, ResponseTypeNames.Code},
                state);

            Assert.True(result == AuthorizationFlow.HybridFlow);
        }

        private void InitializeFakeObjects()
        {
            _authorizationFlowHelper = new AuthorizationFlowHelper();
        }
    }
}
