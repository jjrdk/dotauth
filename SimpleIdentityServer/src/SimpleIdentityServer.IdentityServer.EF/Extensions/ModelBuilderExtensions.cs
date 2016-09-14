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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SimpleIdentityServer.IdentityServer.EF.Models;

namespace SimpleIdentityServer.IdentityServer.EF.Extensions
{
    internal static class ModelBuilderExtensions
    {
        #region Public static methods

        public static void ConfigureUserContext(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(user =>
            {
                user.ToTable(Constants.TableNames.User).HasKey(x => x.Subject);
                user.HasMany(x => x.Claims).WithOne(x => x.User).IsRequired().OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Claim>(claim =>
            {
                claim.ToTable(Constants.TableNames.Claim).HasKey(c => c.Id);
            });
        }

        #endregion
    }
}