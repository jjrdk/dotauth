#region copyright
// Copyright 2015 Habart Thierry
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
#endregion

using Moq;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Manager.Core.Api.ResourceOwners.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.ResourceOwners
{
    public class UpdateResourceOwnerClaimsActionFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IUpdateResourceOwnerClaimsAction _updateResourceOwnerClaimsAction;

        [Fact]
        public async Task When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateResourceOwnerClaimsAction.Execute(null));
        }

        [Fact]
        public async Task When_ResourceOwner_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string subject = "invalid_subject";
            var request = new UpdateResourceOwnerClaimsParameter
            {
                Login = subject
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _updateResourceOwnerClaimsAction.Execute(request));

            // ASSERT
            Assert.NotNull(exception);
            Assert.Equal(ErrorCodes.InvalidParameterCode, exception.Code);
            Assert.Equal(string.Format(ErrorDescriptions.TheResourceOwnerDoesntExist, subject), exception.Message);
        }

        [Fact]
        public async Task When_Resource_Owner_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            var request = new UpdateResourceOwnerClaimsParameter
            {
                Login = "subject"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(false));

            // ACT
            var result = await Assert.ThrowsAsync<IdentityServerManagerException>(() => _updateResourceOwnerClaimsAction.Execute(request));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("internal_error", result.Code);
            Assert.Equal("the claims cannot be updated", result.Message);
        }

        [Fact]
        public async Task When_Updating_Resource_Owner_Then_Operation_Is_Called()
        {
            // ARRANGE
            var request = new UpdateResourceOwnerClaimsParameter
            {
                Login = "subject"
            };
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner()));
            _resourceOwnerRepositoryStub.Setup(r => r.UpdateAsync(It.IsAny<ResourceOwner>())).Returns(Task.FromResult(true));
            _claimRepositoryStub.Setup(c => c.GetAllAsync()).Returns(Task.FromResult((IEnumerable<ClaimAggregate>)new List<ClaimAggregate>()));

            // ACT
            await _updateResourceOwnerClaimsAction.Execute(request);

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(r => r.UpdateAsync(It.IsAny<ResourceOwner>()));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _updateResourceOwnerClaimsAction = new UpdateResourceOwnerClaimsAction(
                _resourceOwnerRepositoryStub.Object, _claimRepositoryStub.Object);
        }
    }
}
