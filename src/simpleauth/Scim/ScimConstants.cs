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

namespace SimpleAuth.Scim
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public static class ScimConstants
    {
        public static class RoutePaths
        {
            public const string GroupsController = "Groups";
            public const string UsersController = "Users";
            public const string SchemasController = "Schemas";
            public const string ServiceProviderConfigController = "ServiceProviderConfig";
            public const string BulkController = "Bulk";
        }

        public static Dictionary<string, string> MappingRoutePathsToResourceTypes = new Dictionary<string, string>
        {
            {
                RoutePaths.UsersController,
                SimpleAuth.Shared.ScimConstants.ResourceTypes.User
            },
            {
                RoutePaths.GroupsController,
                SimpleAuth.Shared.ScimConstants.ResourceTypes.Group
            }
        };
    }
}
