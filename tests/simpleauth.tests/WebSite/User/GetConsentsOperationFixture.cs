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

namespace SimpleAuth.Tests.WebSite.User
{
    using Moq;
    using Shared.Models;
    using Shared.Repositories;
    using SimpleAuth;
    using SimpleAuth.WebSite.User.Actions;
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Shared;
    using Xunit;

    public class GetConsentsOperationFixture
    {
        private Mock<IConsentRepository> _consentRepositoryStub;
        private GetConsentsOperation _getConsentsOperation;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            InitializeFakeObjects();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _getConsentsOperation.Execute(null))
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Getting_Consents_A_List_Is_Returned()
        {
            const string subject = "subject";
            InitializeFakeObjects();
            var claims = new List<Claim>
            {
                new Claim(JwtConstants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            IEnumerable<Consent> consents = new List<Consent>
            {
                new Consent
                {
                    Id = "consent_id"
                }
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            _consentRepositoryStub.Setup(c => c.GetConsentsForGivenUserAsync(subject))
                .Returns(Task.FromResult(consents));

            var result = await _getConsentsOperation.Execute(claimsPrincipal).ConfigureAwait(false);

            Assert.NotNull(result);
            Assert.True(result == consents);
        }

        private void InitializeFakeObjects()
        {
            _consentRepositoryStub = new Mock<IConsentRepository>();
            _getConsentsOperation = new GetConsentsOperation(_consentRepositoryStub.Object);
        }
    }
}
