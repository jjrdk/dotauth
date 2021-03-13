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

namespace SimpleAuth.ViewModels
{
    using SimpleAuth.Shared.Models;

    /// <summary>
    /// Defines the resource set view model.
    /// </summary>
    public class ResourceSetViewModel
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Creates a <see cref="ResourceSetViewModel"/> instance from a <see cref="ResourceSet"/> instance.
        /// </summary>
        /// <param name="resourceSet"></param>
        /// <returns></returns>
        public static ResourceSetViewModel FromResourceSet(ResourceSet resourceSet)
        {
            return new()
            {
                Id = resourceSet.Id,
                Icon = resourceSet.IconUri?.AbsoluteUri,
                Description = resourceSet.Description,
                Name = resourceSet.Name
            };
        }
    }
}