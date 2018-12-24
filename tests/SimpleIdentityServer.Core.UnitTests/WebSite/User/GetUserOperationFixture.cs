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
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    using System.Threading;
    using SimpleAuth.Jwt;
    using SimpleAuth.Shared.Models;
    using SimpleAuth.Shared.Repositories;

    public class GetUserOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private IGetUserOperation _getUserOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _getUserOperation.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_User_Is_Not_Authenticated_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var emptyClaimsPrincipal = new ClaimsPrincipal();

                        var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(emptyClaimsPrincipal)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
        }

        [Fact]
        public async Task When_Subject_Is_Not_Passed_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim("test", "test"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        var exception = await  Assert.ThrowsAsync<IdentityServerException>(() => _getUserOperation.Execute(claimsPrincipal)).ConfigureAwait(false);

                        Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.UnhandledExceptionCode);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheSubjectCannotBeRetrieved);
        }
        
        [Fact]
        public void When_Correct_Subject_Is_Passed_Then_ResourceOwner_Is_Returned()
        {            InitializeFakeObjects();
            var claimsIdentity = new ClaimsIdentity("test");
            claimsIdentity.AddClaim(new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, "subject"));
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _resourceOwnerRepositoryStub.Setup(r => r.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceOwner()));

                        var result = _getUserOperation.Execute(claimsPrincipal);

                        Assert.NotNull(result);
        }
        
        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _getUserOperation = new GetUserOperation(_resourceOwnerRepositoryStub.Object);
        }
    }
}
