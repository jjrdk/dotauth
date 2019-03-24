// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
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

namespace SimpleAuth.Tests.Authenticate
{
    using Shared.Models;
    using SimpleAuth.Authenticate;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using Xunit;

    public class ClientTlsAuthenticationFixture
    {
        [Fact]
        public void WhenBothParametersAreNullThenThrows()
        {
            Assert.Throws<NullReferenceException>(() => ClientTlsAuthentication.AuthenticateClient(null, null));
        }

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            Assert.Throws<NullReferenceException>(
                () => ClientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction(), null));
        }

        [Fact]
        public void When_Passing_NoSecret_Or_Certificate_Then_Null_Is_Returned()
        {
            Assert.Null(ClientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction(), new Client()));
            Assert.Null(
                ClientTlsAuthentication.AuthenticateClient(
                    new AuthenticateInstruction {Certificate = new X509Certificate2()},
                    new Client()));
        }

        [Fact]
        public void When_Client_Does_Not_Contain_ThumbprintAndName_Then_Null_Is_Returned()
        {
            Assert.Null(
                ClientTlsAuthentication.AuthenticateClient(
                    new AuthenticateInstruction {Certificate = new X509Certificate2()},
                    new Client()));
        }
    }
}
