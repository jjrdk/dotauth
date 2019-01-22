//// Copyright © 2018 Habart Thierry, © 2018 Jacob Reimers
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.

//using Newtonsoft.Json.Linq;
//using System.Linq;
//using System.Net;
//using System.Threading.Tasks;
//using Xunit;

//namespace SimpleIdentityServer.Scim.Client.Tests
//{
//    using SimpleIdentityServer.Core.Common;
//    using SimpleIdentityServer.Core.Common.DTOs;
//    using SimpleIdentityServer.Core.Common.Models;
//    using System;

//    public class GroupsClientFixture : IClassFixture<TestScimServerFixture>
//    {
//        private readonly TestScimServerFixture _testScimServerFixture;
//        private IGroupsClient _groupsClient;

//        public GroupsClientFixture(TestScimServerFixture testScimServerFixture)
//        {
//            _testScimServerFixture = testScimServerFixture;
//        }

//        [Fact]
//        public async Task WhenSettingImmutablePropertyThenFails()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            var patchOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = ScimConstants.GroupResourceResponseNames.Members,
//                Value = JArray.Parse("[{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value'},{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group2','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value2'}]")
//            };

//            var removeGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.remove,
//                Path = "members[type eq group2]"
//            };

//            var addGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.add,
//                Path = "members",
//                Value = JArray.Parse("[{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group3','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value3'}]")
//            };
//            var updateGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = "members[type eq group3].value",
//                Value = "new_value"
//            };

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // ACT : Update group
//            var updateResponse = await _groupsClient.UpdateGroup(
//                       new Uri(baseUrl),
//                       id,
//                       new GroupResource { Id = "other_id" })
//                   .ConfigureAwait(false);

//            id = updateResponse.Content["id"].ToString();

//            // 4 ACT : Partial update group
//            var patchResult = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, patchOperation).ConfigureAwait(false);

//            // 5 ACT : Remove group2
//            await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, removeGroupOperation)
//                .ConfigureAwait(false);

//            // 6 ACT : Add group3
//            await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, addGroupOperation).ConfigureAwait(false);

//            // ACT : Update the group3 type (immutable property)
//            var sevenResult = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, updateGroupOperation)
//                .ConfigureAwait(false);

//            //            Assert.NotNull(sevenResult);
//            Assert.True(sevenResult.StatusCode == HttpStatusCode.BadRequest);
//        }

//        [Fact]
//        public async Task WhenCreatingGroupThenReturnsOk()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            //            Assert.NotNull(firstResult);
//            Assert.True(firstResult.StatusCode == HttpStatusCode.Created);
//            var id = firstResult.Content["id"].ToString();

//            // ACT : Get group
//            var secondResult = await _groupsClient.GetGroup(new Uri(baseUrl), id).ConfigureAwait(false);

//            //            Assert.NotNull(secondResult);
//            Assert.True(secondResult.StatusCode == HttpStatusCode.OK);
//            Assert.True(secondResult.Content["id"].ToString() == id);
//        }

//        [Fact]
//        public async Task WhenDeletingGroupThenReturnsOk()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // ACT : Remove group
//            var thenResult = await _groupsClient.DeleteGroup(new Uri(baseUrl), id).ConfigureAwait(false);

//            //            Assert.NotNull(thenResult);
//            Assert.Equal(HttpStatusCode.NoContent, thenResult.StatusCode);
//        }

//        [Fact]
//        public async Task WhenSearchingGroupsThenReturnsOnlyMembers()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            var patchOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = ScimConstants.GroupResourceResponseNames.Members,
//                Value = JArray.Parse(
//                    $"[{{\'{ScimConstants.MultiValueAttributeNames.Type}\' : \'group\',\'{ScimConstants.MultiValueAttributeNames.Value}\' : \'value\'}},{{\'{ScimConstants.MultiValueAttributeNames.Type}\' : \'group2\',\'{ScimConstants.MultiValueAttributeNames.Value}\' : \'value2\'}}]")
//            };

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // 4 ACT : Partial update group
//            await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, patchOperation).ConfigureAwait(false);

//            // ACT : Get only members
//            var nineResult = await _groupsClient.SearchGroups(
//                    new Uri(baseUrl),
//                    new SearchParameter
//                    {
//                        Filter = "members[type pr]",
//                        Attributes = new[] { "members.type" }
//                    })
//                .ConfigureAwait(false);

//            //            Assert.NotNull(nineResult);
//        }

//        [Fact]
//        public async Task WhenAddingMultipleGroupsThenCanQueryAndReceiveAllGroups()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            // ACT : Add ten groups
//            for (var i = 0; i < 10; i++)
//            {
//                await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);
//            }

//            // ACT : Get 10 groups
//            var eightResult = await _groupsClient.SearchGroups(new Uri(baseUrl),
//                    new SearchParameter
//                    {
//                        StartIndex = 0,
//                        Count = 10
//                    })
//                .ConfigureAwait(false);

//            //            Assert.NotNull(eightResult);
//            Assert.True(eightResult.Content["Resources"].Count() == 10);
//        }

//        [Fact]
//        public async Task CanUpdateExistingGroup()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // ACT : Update group
//            var thirdResult = await _groupsClient.UpdateGroup(new Uri(baseUrl),
//                    id,
//                    new GroupResource { Id = "other_id" })
//                .ConfigureAwait(false);

//            //            Assert.NotNull(thirdResult);
//            Assert.True(thirdResult.StatusCode == HttpStatusCode.OK);
//            Assert.True(thirdResult.Content["id"].ToString() == id);
//            Assert.True(thirdResult.Content[ScimConstants.GroupResourceResponseNames.DisplayName].ToString() ==
//                        "display_name");
//            Assert.True(thirdResult.Content[ScimConstants.IdentifiedScimResourceNames.ExternalId].ToString() ==
//                        "other_id");
//        }

//        [Fact]
//        public async Task CanPartiallyUpdateExistingGroup()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            var patchOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = ScimConstants.GroupResourceResponseNames.Members,
//                Value = JArray.Parse("[{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value'},{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group2','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value2'}]")
//            };

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);


//            var id = firstResult.Content["id"].ToString();

//            // ACT : Partial update group
//            var fourthResult = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, patchOperation)
//                .ConfigureAwait(false);

//            //            Assert.NotNull(fourthResult);
//            Assert.True(fourthResult.StatusCode == HttpStatusCode.OK);
//            Assert.True(fourthResult.Content["id"].ToString() == id);
//            Assert.True(
//                fourthResult.Content[ScimConstants.GroupResourceResponseNames.Members][0][ScimConstants
//                        .MultiValueAttributeNames.Type]
//                    .ToString() ==
//                "group");
//            Assert.True(
//                fourthResult.Content[ScimConstants.GroupResourceResponseNames.Members][0][ScimConstants
//                        .MultiValueAttributeNames.Value]
//                    .ToString() ==
//                "value");
//        }

//        [Fact]
//        public async Task CanRemoveSpecifiedMemberFromGroup()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            var patchOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = ScimConstants.GroupResourceResponseNames.Members,
//                Value = JArray.Parse("[{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value'},{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group2','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value2'}]")
//            };

//            var removeGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.remove,
//                Path = "members[type eq group2]"
//            };

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // ACT : Update group
//            var updateResponse = await _groupsClient.UpdateGroup(new Uri(baseUrl),
//                    id,
//                    new GroupResource { Id = "other_id" })
//                .ConfigureAwait(false);
//            id = updateResponse.Content["id"].ToString();

//            // 4 ACT : Partial update group
//            await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, patchOperation).ConfigureAwait(false);

//            // ACT : Remove group2
//            var fifthResult = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, removeGroupOperation)
//                .ConfigureAwait(false);

//            Assert.True(fifthResult.Content[ScimConstants.GroupResourceResponseNames.Members].Count() == 1);
//        }

//        [Fact]
//        public async Task CanAddSpecifiedMemberFromGroup()
//        {
//            const string baseUrl = "http://localhost:5555";
////            InitializeFakeObjects();

//            var patchOperation = new PatchOperation
//            {
//                Type = PatchOperations.replace,
//                Path = ScimConstants.GroupResourceResponseNames.Members,
//                Value = JArray.Parse(
//                    "[" +
//                    $"{{\'{ScimConstants.MultiValueAttributeNames.Type}\' : \'group\',\'{ScimConstants.MultiValueAttributeNames.Value}\' : \'value\'}}," +
//                    $"{{\'{ScimConstants.MultiValueAttributeNames.Type}\' : \'group2\',\'{ScimConstants.MultiValueAttributeNames.Value}\' : \'value2\'}}" +
//                    "]")
//            };

//            var removeGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.remove,
//                Path = "members[type eq group2]"
//            };

//            var addGroupOperation = new PatchOperation
//            {
//                Type = PatchOperations.add,
//                Path = "members",
//                Value = JArray.Parse("[{'" +
//                                     ScimConstants.MultiValueAttributeNames.Type +
//                                     "' : 'group3','" +
//                                     ScimConstants.MultiValueAttributeNames.Value +
//                                     "' : 'value3'}]")
//            };

//            // ACT : CreateJwk group
//            var firstResult = await _groupsClient.AddGroup(new Uri(baseUrl), "external_id").ConfigureAwait(false);

//            var id = firstResult.Content["id"].ToString();

//            // ACT : Update group
//            var updateResult = await _groupsClient.UpdateGroup(new Uri(baseUrl),
//                         id,
//                         new GroupResource { Id = "other_id" })
//                     .ConfigureAwait(false);

//            id = updateResult.Content["id"].ToString();

//            // 4 ACT : Partial update group
//            var patch1Result = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, patchOperation).ConfigureAwait(false);

//            // ACT : Remove group2
//            var patch2Result = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, removeGroupOperation)
//                     .ConfigureAwait(false);

//            // ACT : Add group3
//            var sixResult = await _groupsClient.PartialUpdateGroup(new Uri(baseUrl), id, null, addGroupOperation)
//                .ConfigureAwait(false);

//            Assert.NotNull(sixResult);
//            Assert.True(sixResult.Content[ScimConstants.GroupResourceResponseNames.Members].Count() == 2);
//        }

//        private void InitializeFakeObjects()
//        {
//            _groupsClient = new GroupsClient(_testScimServerFixture.Client);
//        }
//    }
//}
