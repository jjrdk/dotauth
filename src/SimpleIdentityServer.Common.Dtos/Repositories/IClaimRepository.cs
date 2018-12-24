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

namespace SimpleIdentityServer.Shared.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models;
    using Parameters;
    using Results;

    public interface IClaimRepository
    {
        Task<SearchClaimsResult> Search(SearchClaimsParameter parameter);
        Task<IEnumerable<ClaimAggregate>> GetAllAsync();
        Task<ClaimAggregate> GetAsync(string name);
        Task<bool> InsertAsync(AddClaimParameter claim);
        Task<bool> Delete(string code);
        Task<bool> Update(ClaimAggregate claim);
    }
}
