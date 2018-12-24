namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws
{
    using Moq;
    using SimpleIdentityServer.Core.Api.Jws;
    using SimpleIdentityServer.Core.Api.Jws.Actions;
    using SimpleIdentityServer.Core.Parameters;
    using System;
    using System.Threading.Tasks;
    using SimpleAuth.Shared;
    using Xunit;

    public class JwsActionsFixture
    {
        private Mock<IGetJwsInformationAction> _getJwsInformationActionStub;
        private Mock<ICreateJwsAction> _createJwsActionStub;
        private IJwsActions _jwsActions;

        [Fact]
        public async Task When_Passing_Null_Parameter_To_GetJwsInformation_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(getJwsParameter)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Null_Parameter_To_CreateJws_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.CreateJws(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Executing_GetJwsInformation_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };

                        await _jwsActions.GetJwsInformation(getJwsParameter).ConfigureAwait(false);

                        _getJwsInformationActionStub.Verify(g => g.Execute(getJwsParameter));
        }

        [Fact]
        public async Task When_Executing_CreateJws_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload()
            };

                        await _jwsActions.CreateJws(createJwsParameter).ConfigureAwait(false);

                        _createJwsActionStub.Verify(g => g.Execute(createJwsParameter));
        }

        private void InitializeFakeObjects()
        {
            _getJwsInformationActionStub = new Mock<IGetJwsInformationAction>();
            _createJwsActionStub = new Mock<ICreateJwsAction>();
            _jwsActions = new JwsActions(
                _getJwsInformationActionStub.Object,
                _createJwsActionStub.Object);
        }
    }
}
