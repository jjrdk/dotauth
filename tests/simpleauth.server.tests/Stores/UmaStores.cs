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

namespace SimpleAuth.Server.Tests.Stores
{
    using System.Collections.Generic;
    using SimpleAuth.Shared.Models;

    public static class UmaStores
    {
        public static List<ResourceSetModel> GetResources()
        {
            return new List<ResourceSetModel>
            {
                new ResourceSetModel
                {
                    Owner = "tester", Id = "bad180b5-4a96-422d-a088-c71a9f7c7afc", Name = "Resources"
                },
                new ResourceSetModel {Owner = "tester", Id = "67c50eac-23ef-41f0-899c-dffc03add961", Name = "Apis"}
            };
        }
    }
}
