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

namespace SimpleAuth.Tests.Api.Clients.Actions
{
    //public class GetClientActionFixture
    //{
    //    private Mock<IClientStore> _clientRepositoryStub;

    //    [Fact]
    //    public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
    //    {
    //        InitializeFakeObjects();

    //        await Assert.ThrowsAsync<ArgumentNullException>(() => _clientRepositoryStub.Object.GetById(null)).ConfigureAwait(false);
    //    }

    //    [Fact]
    //    public async Task When_Client_Doesnt_Exist_Then_Exception_Is_Thrown()
    //    {
    //        const string clientId = "client_id";
    //        InitializeFakeObjects();
    //        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
    //            .Returns(Task.FromResult((Client)null));

    //        
    //        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _clientRepositoryStub.Object.GetById(clientId)).ConfigureAwait(false);
    //        Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
    //        Assert.True(exception.Message == string.Format(ErrorDescriptions.TheClientDoesntExist, clientId));
    //    }

    //    [Fact]
    //    public async Task When_Getting_Client_Then_Information_Are_Returned()
    //    {
    //        const string clientId = "clientId";
    //        var client = new Client
    //        {
    //            ClientId = clientId
    //        };
    //        InitializeFakeObjects();
    //        _clientRepositoryStub.Setup(c => c.GetById(It.IsAny<string>()))
    //            .Returns(Task.FromResult(client));

    //        var result = await _clientRepositoryStub.Object.GetById(clientId).ConfigureAwait(false);

    //        Assert.NotNull(result);
    //        Assert.True(result.ClientId == clientId);
    //    }

    //    private void InitializeFakeObjects()
    //    {
    //        _clientRepositoryStub = new Mock<IClientStore>();
    //    }
    //}
}
