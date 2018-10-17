// Copyright 2016 Habart Thierry
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

using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Scim.Client.Tests.MiddleWares;
using SimpleIdentityServer.Scim.Core.EF.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Scim.Client.Tests
{
    using SimpleIdentityServer.Core.Common;
    using SimpleIdentityServer.Core.Common.Models;

    public class UsersClientFixture : IClassFixture<TestScimServerFixture>
    {
        private const string baseUrl = "http://localhost:5555";
        private readonly TestScimServerFixture _testScimServerFixture;
        private IUsersClient _usersClient;

        public UsersClientFixture(TestScimServerFixture testScimServerFixture)
        {
            _testScimServerFixture = testScimServerFixture;
        }

        [Fact]
        public async Task When_Add_Authenticated_User_Then_ScimIdentifier_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var scimResponse = await _usersClient.AddAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);

            // ASSERTS
            Assert.Equal(HttpStatusCode.Created, scimResponse.StatusCode);
        }

        [Fact]
        public async Task When_Update_Current_User_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var scimResponse = await _usersClient.AddAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            var scimId = scimResponse.Content["id"].ToString();
            UserStore.Instance().ScimId = scimId;
            var thirdResult = await _usersClient.UpdateAuthenticatedUser(new Uri(baseUrl),
                    scimId,
                    new JProperty(ScimConstants.UserResourceResponseNames.UserName, "other_username"))
                .ConfigureAwait(false);
            UserStore.Instance().ScimId = null;

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, thirdResult.StatusCode);
        }

        [Fact]
        public async Task When_Partially_Update_Current_User_Then_Ok_Is_Returned()
        {
            // ARRANGE
            var patchOperation = new PatchOperation
            {
                Path = ScimConstants.UserResourceResponseNames.UserName,
                Type = PatchOperations.replace,
                Value = "new_username"
            };
            //new PatchOperationBuilder().SetType(PatchOperations.replace)
            //.SetPath(ScimConstants.UserResourceResponseNames.UserName)
            //.SetContent("new_username")
            //.Build();
            InitializeFakeObjects();

            // ACT
            var scimResponse = await _usersClient.AddAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            var scimId = scimResponse.Content["id"].ToString();
            UserStore.Instance().ScimId = scimId;
            var thirdResult = await _usersClient.PartialUpdateAuthenticatedUser(new Uri(baseUrl), scimId, patchOperation).ConfigureAwait(false);
            //.AddOperation(patchOperation)
            //.Execute();
            UserStore.Instance().ScimId = null;

            // ASSERT
            Assert.Equal(HttpStatusCode.OK, thirdResult.StatusCode);
        }

        [Fact]
        public async Task When_Remove_Current_User_Then_NoContent_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var scimResponse = await _usersClient.AddAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            var scimId = scimResponse.Content["id"].ToString();
            UserStore.Instance().ScimId = scimId;
            var removeResponse = await _usersClient.DeleteAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            UserStore.Instance().ScimId = null;

            // ASSERTS
            Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);
        }

        [Fact]
        public async Task When_Get_Authenticated_User_Then_Ok_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var scimResponse = await _usersClient.AddAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            var scimId = scimResponse.Content["id"].ToString();
            UserStore.Instance().ScimId = scimId;
            var userResponse = await _usersClient.GetAuthenticatedUser(new Uri(baseUrl), "token").ConfigureAwait(false);
            UserStore.Instance().ScimId = null;


            // ASSERTS
            Assert.Equal(HttpStatusCode.OK, userResponse.StatusCode);
        }

        [Fact]
        public async Task When_Insert_Ten_Users_And_Search_Two_Users_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            for (var i = 0; i < 10; i++)
            {
                await _usersClient.AddUser(new Uri(baseUrl),
                        "external_id",
                        null,
                        new JProperty(ScimConstants.UserResourceResponseNames.UserName, "username"))
                    .ConfigureAwait(false);
                //.SetCommonAttributes("external_id")
                //.AddAttribute()
                //.Execute();
            }

            var searchResult = await _usersClient.SearchUsers(
                new Uri(baseUrl),
                new SearchParameter
                {
                    StartIndex = 0,
                    Count = 2
                }).ConfigureAwait(false);

            // ASSERTS
            Assert.True(searchResult.Content["Resources"].Count() == 2);
        }

        [Fact]
        public async Task When_Insert_Complex_Users_Then_Information_Are_Correct()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jArr = new JArray
            {
                "a1",
                "a2"
            };
            var jObj = new JObject
            {
                { ScimConstants.NameResponseNames.MiddleName, "middlename" },
                { ScimConstants.NameResponseNames.GivenName, "givename" }
            };
            var complexArr = new JArray();
            var complexObj = new JObject
            {
                { "test", "test2" }
            };
            complexArr.Add(complexObj);
            var firstResult = await _usersClient.AddUser(new Uri(baseUrl),
                null,
                null,
                new JProperty(ScimConstants.UserResourceResponseNames.UserName, "username"),
                new JProperty("arr", jArr),
                new JProperty("date", DateTime.UtcNow),
                new JProperty("age", 23),
                new JProperty("complexarr", complexArr),
                new JProperty(ScimConstants.UserResourceResponseNames.Name, jObj)).ConfigureAwait(false);
            var id = firstResult.Content["id"].ToString();
            Assert.NotNull(id);

            var firstSearch = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"arr co a1"
            }).ConfigureAwait(false);
            var secondSearch = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"complexarr[test eq test2]"
            }).ConfigureAwait(false);
            var thirdSearch = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"age le 23"
            }).ConfigureAwait(false);
            var newDate = DateTime.UtcNow.AddDays(2).ToUnix().ToString();
            var fourthSearch = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                StartIndex = 0,
                Count = 10,
                Filter = $"date lt {newDate}"
            }).ConfigureAwait(false);

            Assert.NotNull(firstSearch);
            Assert.NotNull(secondSearch);
            Assert.NotNull(thirdSearch);
            Assert.NotNull(fourthSearch);

            //var eightResult = await _usersClient.DeleteUser(baseUrl, id);
        }

        [Fact]
        public async Task When_Execute_Operations_On_Users_Then_No_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var patchOperation = new PatchOperation
            {
                Path = ScimConstants.UserResourceResponseNames.UserName,
                Type = PatchOperations.replace,
                Value = "new_username"
            };
            //new PatchOperationBuilder().SetType(PatchOperations.replace)
            //.SetPath(ScimConstants.UserResourceResponseNames.UserName)
            //.SetContent("new_username")
            //.Build();
            var addEmailsOperation = new PatchOperation
            {
                Value = JArray.Parse("[{'" +
                                     ScimConstants.MultiValueAttributeNames.Type +
                                     "' : 'work','" +
                                     ScimConstants.MultiValueAttributeNames.Value +
                                     "' : 'bjensen@example.com'}, {'" +
                                     ScimConstants.MultiValueAttributeNames.Type +
                                     "' : 'home','" +
                                     ScimConstants.MultiValueAttributeNames.Value +
                                     "' : 'bjensen@example.com'}]"),
                Path = ScimConstants.UserResourceResponseNames.Emails,
                Type = PatchOperations.replace
            };
            //new PatchOperationBuilder().SetType(PatchOperations.replace)
            //.SetPath(ScimConstants.UserResourceResponseNames.Emails)
            //.SetContent(JArray.Parse("[{'" + ScimConstants.MultiValueAttributeNames.Type + "' : 'work','" + ScimConstants.MultiValueAttributeNames.Value + "' : 'bjensen@example.com'}, {'" + ScimConstants.MultiValueAttributeNames.Type + "' : 'home','" + ScimConstants.MultiValueAttributeNames.Value + "' : 'bjensen@example.com'}]"))
            //.Build();
            var removeEmailOperation = new PatchOperation
            {
                Path = "emails[type eq work]",
                Type = PatchOperations.remove
            };
            //new PatchOperationBuilder().SetType(PatchOperations.remove)
            //.SetPath("emails[type eq work]")
            //.Build();

            // ACT : Create user
            var firstResult = await _usersClient.AddUser(new Uri(baseUrl),
                    "external_id",
                    null,
                    new JProperty(ScimConstants.UserResourceResponseNames.UserName, "username"))
                .ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(firstResult);
            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
            var id = firstResult.Content["id"].ToString();

            // ACT : Partial update user
            var secondResult = await _usersClient.PartialUpdateUser(new Uri(baseUrl), id, null, patchOperation).ConfigureAwait(false);
                //.AddOperation(patchOperation)
                //.Execute();

            // ASSERTS
            Assert.NotNull(secondResult);
            Assert.True(secondResult.Content[ScimConstants.UserResourceResponseNames.UserName].ToString() == "new_username");

            // ACT : Update user
            var thirdResult = await _usersClient.UpdateUser(new Uri(baseUrl),
                id,
                null,
                new JProperty(ScimConstants.IdentifiedScimResourceNames.ExternalId, "new_external_id"),
                new JProperty(ScimConstants.UserResourceResponseNames.UserName, "other_username"),
                new JProperty(ScimConstants.UserResourceResponseNames.Active, "false")).ConfigureAwait(false);
                //.SetCommonAttributes("new_external_id")
                //.AddAttribute(new JProperty(ScimConstants.UserResourceResponseNames.UserName, "other_username"))
                //.AddAttribute(new JProperty(ScimConstants.UserResourceResponseNames.Active, "false"))
                //.Execute();

            // ASSERTS
            Assert.NotNull(thirdResult);
            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
            Assert.True(thirdResult.Content[ScimConstants.UserResourceResponseNames.UserName].ToString() == "other_username");
            var active = thirdResult.Content[ScimConstants.UserResourceResponseNames.Active].ToString();
            Assert.False(bool.Parse(active));
            Assert.True(thirdResult.Content[ScimConstants.IdentifiedScimResourceNames.ExternalId].ToString() == "new_external_id");

            // ACT : Add emails to the user
            var fourthResult = await _usersClient.PartialUpdateUser(new Uri(baseUrl), id, null, addEmailsOperation).ConfigureAwait(false);
                //.AddOperation()
                //.Execute();

            // ASSERTS
            Assert.NotNull(fourthResult);
            Assert.True(fourthResult.StatusCode == HttpStatusCode.OK);
            Assert.True(fourthResult.Content[ScimConstants.UserResourceResponseNames.Emails].Count() == 2);

            // ACT : Remove emails of the user
            var fifthResult = await _usersClient.PartialUpdateUser(new Uri(baseUrl), id, null, removeEmailOperation).ConfigureAwait(false);
                //.AddOperation()
                //.Execute();

            // ASSERTS
            Assert.NotNull(fifthResult);
            Assert.True(fifthResult.StatusCode == HttpStatusCode.OK);
            Assert.True(fifthResult.Content[ScimConstants.UserResourceResponseNames.Emails].Count() == 1);

            // ACT : Add 10 users
            for (int i = 0; i < 10; i++)
            {
                await _usersClient.AddUser(
                    new Uri(baseUrl),
                    properties: new[]
                    {
                        new JProperty(ScimConstants.IdentifiedScimResourceNames.ExternalId, "new_external_id"),
                        new JProperty(ScimConstants.UserResourceResponseNames.UserName, Guid.NewGuid().ToString())
                    }).ConfigureAwait(false);
                //.SetCommonAttributes(Guid.NewGuid().ToString())
                //.AddAttribute(new JProperty(ScimConstants.UserResourceResponseNames.UserName, Guid.NewGuid().ToString()))
                //.Execute();
            }

            // ACT : Get 10 users
            var sixResult = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                StartIndex = 0,
                Count = 10
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(sixResult);
            var c = sixResult.Content["Resources"];
            Assert.True(sixResult.Content["Resources"].Count() == 10);

            // ACT : Get only emails
            var sevenResult = await _usersClient.SearchUsers(new Uri(baseUrl), new SearchParameter
            {
                Filter = "emails[type pr]",
                Attributes = new[] { "emails.type", "emails.value", "emails.display", "userName" }
            }).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(sevenResult);

            // ACT : Remove the user
            var eightResult = await _usersClient.DeleteUser(new Uri(baseUrl), id).ConfigureAwait(false);

            // ASSERTS
            Assert.NotNull(eightResult);
            Assert.True(eightResult.StatusCode == HttpStatusCode.NoContent);
        }

        private void InitializeFakeObjects()
        {
            _usersClient = new UsersClient(_testScimServerFixture.Client);
        }
    }
}
