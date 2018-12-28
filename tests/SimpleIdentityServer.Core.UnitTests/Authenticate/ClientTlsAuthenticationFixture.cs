// Copyright 2017 Habart Thierry
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
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using Shared.Models;
    using SimpleAuth.Authenticate;
    using Xunit;

    public class ClientTlsAuthenticationFixture
    {
        private IClientTlsAuthentication _clientTlsAuthentication;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exceptions_Are_Thrown()
        {            InitializeFakeObjects();

            
            Assert.Throws<ArgumentNullException>(() => _clientTlsAuthentication.AuthenticateClient(null, null));
            Assert.Throws<ArgumentNullException>(() => _clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction(), null));
        }

        [Fact]
        public void When_Passing_NoSecret_Or_Certificate_Then_Null_Is_Returned()
        {            InitializeFakeObjects();

            
            Assert.Null(_clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction(), new Client()));
            Assert.Null(_clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction
            {
                Certificate = new X509Certificate2()
            }, new Client()));
        }

        [Fact]
        public void When_Client_Doesnt_Contain_ThumbprintAndName_Then_Null_Is_Returned()
        {            InitializeFakeObjects();

            Assert.Null(_clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction
            {
                Certificate = new X509Certificate2()
            }, new Client
            {
                Secrets = new List<ClientSecret>()
            }));
        }

        /*
        [Fact]
        public void When_ThumbPrint_Is_Not_Correct_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");

            Assert.Null(_clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction
            {
                Certificate = certificate
            }, new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Thumbprint,
                        Value = "not_correct"
                    }
                }
            }));
        }

        [Fact]
        public void When_Name_Is_Not_Correct_Then_Null_Is_Returned()
        {            InitializeFakeObjects();
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");

            Assert.Null(_clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction
            {
                Certificate = certificate
            }, new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Name,
                        Value = "not_correct"
                    }
                }
            }));
        }

        [Fact]
        public void When_Client_Is_Authenticated_Then_Client_Is_Returned()
        {            InitializeFakeObjects();
            var certificate = new X509Certificate2("testCert.pfx", "testPassword");

                        var result = _clientTlsAuthentication.AuthenticateClient(new AuthenticateInstruction
            {
                Certificate = certificate
            }, new Client
            {
                Secrets = new List<ClientSecret>
                {
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Name,
                        Value = certificate.SubjectName.Name
                    },
                    new ClientSecret
                    {
                        Type = ClientSecretTypes.X509Thumbprint,
                        Value = certificate.Thumbprint
                    }
                }
            });

                        Assert.NotNull(result);
        }
        */

        private void InitializeFakeObjects()
        {
            _clientTlsAuthentication = new ClientTlsAuthentication();
        }
    }
}
