// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Moq;
using SimpleIdentityServer.Core.Api.Introspection;
using SimpleIdentityServer.Core.Api.Introspection.Actions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Introspection
{
    using Shared;

    public class IntrospectionActionsFixture
    {
        private Mock<IPostIntrospectionAction> _postIntrospectionActionStub;
        private Mock<IPayloadSerializer> _payloadSerializerStub;
        private Mock<IIntrospectionParameterValidator> _validatorStub;
        private IIntrospectionActions _introspectionActions;

        [Fact]
        public async Task When_Passing_Null_Parameter_To_PostIntrospection_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _introspectionActions.PostIntrospection(null, null, null)).ConfigureAwait(false);
        }

        [Fact]
        public void When_Passing_Valid_Parameter_To_PostIntrospection_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
            var parameter = new IntrospectionParameter();

                        _introspectionActions.PostIntrospection(parameter, null, null);

                        _postIntrospectionActionStub.Verify(p => p.Execute(It.IsAny<IntrospectionParameter>(),
                It.IsAny<AuthenticationHeaderValue>(), null));
        }

        private void InitializeFakeObjects()
        {
            _postIntrospectionActionStub = new Mock<IPostIntrospectionAction>();
            var eventPublisherStub = new Mock<IEventPublisher>();
            _payloadSerializerStub = new Mock<IPayloadSerializer>();
            _validatorStub = new Mock<IIntrospectionParameterValidator>();
            _introspectionActions = new IntrospectionActions(_postIntrospectionActionStub.Object, eventPublisherStub.Object,
                _payloadSerializerStub.Object, _validatorStub.Object);
        }
    }
}
