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
using SimpleIdentityServer.Manager.Core.Tests.Fake;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Helpers
{
    using SimpleAuth;
    using SimpleAuth.Converter;
    using SimpleAuth.Errors;
    using SimpleAuth.Exceptions;
    using SimpleAuth.Helpers;
    using SimpleAuth.Shared;
    using SimpleAuth.Shared.Requests;

    public class JsonWebKeyHelperFixture
    {
        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterStub;
        private IJsonWebKeyHelper _jsonWebKeyHelper;

        [Fact]
        public async Task When_Passing_No_Kid_To_GetJsonWebKey_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects(new HttpClient());

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _jsonWebKeyHelper.GetJsonWebKey(null, null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_No_Uri_To_GetJsonWebKey_Then_Exception_Is_Thrown()
        {            InitializeFakeObjects(new HttpClient());

                        await Assert.ThrowsAsync<ArgumentNullException>(() => _jsonWebKeyHelper.GetJsonWebKey("kid", null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_The_JsonWebKey_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("json")
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            InitializeFakeObjects(new HttpClient(handler));
            const string url = "http://google.be/";
            const string kid = "kid";
            var uri = new Uri(url);

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerException>(async () => await _jsonWebKeyHelper.GetJsonWebKey(kid, uri).ConfigureAwait(false)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_Requesting_JsonWeb_Key_Then_Its_Information_Are_Returned()
        {
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            InitializeFakeObjects(new HttpClient(handler));
            const string url = "http://google.be/";
            const string kid = "kid";
            var uri = new Uri(url);
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = kid
                }
            };

            _jsonWebKeyConverterStub.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

                        var result = await _jsonWebKeyHelper.GetJsonWebKey(kid, uri).ConfigureAwait(false);

                        Assert.NotNull(result);
            Assert.True(result.Kid == kid);
        }

        private void InitializeFakeObjects(HttpClient client)
        {
            _jsonWebKeyConverterStub = new Mock<IJsonWebKeyConverter>();
            _jsonWebKeyHelper = new JsonWebKeyHelper(_jsonWebKeyConverterStub.Object, client);
        }
    }
}
