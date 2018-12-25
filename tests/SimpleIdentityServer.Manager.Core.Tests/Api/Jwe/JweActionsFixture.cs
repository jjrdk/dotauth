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

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jwe
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SimpleAuth.Api.Jwe;
    using SimpleAuth.Api.Jwe.Actions;
    using SimpleAuth.Parameters;
    using Xunit;

    public class JweActionsFixture
    {
        private Mock<IGetJweInformationAction> _getJweInformationActionStub;
        private Mock<ICreateJweAction> _createJweActionStub;
        private IJweActions _jweActions;

        [Fact]
        public void When_Passing_Null_Parameter_To_GetJweInformation_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        Assert.ThrowsAsync<ArgumentNullException>(() => _jweActions.GetJweInformation(null));
        }

        [Fact]
        public async Task When_Execute_GetJweInformation_Then_Operation_Is_Called()
        {            InitializeFakeObjects();
            var parameter = new GetJweParameter();

                        await _jweActions.GetJweInformation(parameter).ConfigureAwait(false);

                        _getJweInformationActionStub.Verify(g => g.ExecuteAsync(parameter));
        }

        private void InitializeFakeObjects()
        {
            _getJweInformationActionStub = new Mock<IGetJweInformationAction>();
            _createJweActionStub = new Mock<ICreateJweAction>();
            _jweActions = new JweActions(
                _getJweInformationActionStub.Object,
                _createJweActionStub.Object);
        }
    }
}
