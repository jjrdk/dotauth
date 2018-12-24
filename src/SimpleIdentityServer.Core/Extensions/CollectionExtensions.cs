﻿// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
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

using System.Collections.Generic;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static void AddRange<T, TZ>(this Dictionary<T, TZ> firstDictionary,
            Dictionary<T, TZ> secondDictionary)
        {
            foreach (var keyPair in secondDictionary)
            {
                firstDictionary.Add(keyPair.Key, keyPair.Value);
            }
        }

        public static void AddRange<T, TZ>(this IDictionary<T, TZ> firstDictionary,
            IDictionary<T, TZ> secondDictionary)
        {
            if (secondDictionary == null)
            {
                return;
            }
            foreach (var keyPair in secondDictionary)
            {
                firstDictionary.Add(keyPair.Key, keyPair.Value);
            }
        }
    }
}
