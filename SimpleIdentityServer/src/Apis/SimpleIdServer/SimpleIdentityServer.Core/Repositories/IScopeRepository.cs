﻿#region copyright
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

using System.Collections.Generic;

using SimpleIdentityServer.Core.Models;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IScopeRepository
    {
        Task<SearchScopeResult> Search(SearchScopesParameter parameter);
        Task<Scope> GetAsync(string name);
        Task<ICollection<Scope>> SearchByNamesAsync(IEnumerable<string> names);
        Task<ICollection<Scope>> GetAllAsync();
        Task<bool> InsertAsync(Scope scope);
        Task<bool> DeleteAsync(Scope scope);
        Task<bool> UpdateAsync(Scope scope);
    }
}
