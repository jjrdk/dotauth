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

namespace SimpleIdentityServer.Core.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DTOs;

    public class RepresentationAttribute : ICloneable, IComparable
    {
        public RepresentationAttribute() { }
        public RepresentationAttribute(SchemaAttributeResponse schemaAttribute)
        {
            SchemaAttribute = schemaAttribute;
        }

        public SchemaAttributeResponse SchemaAttribute { get; set; }

        public RepresentationAttribute Parent { get; set; }

        public string Name { get; set; }

        public string FullPath => GetFullPath();

        public object Clone()
        {
            return CloneObj();
        }

        public virtual dynamic GetValue()
        {
            return string.Empty;
        }

        protected virtual object CloneObj()
        {
            return new RepresentationAttribute(SchemaAttribute);
        }

        private string GetFullPath()
        {
            var parents = new List<RepresentationAttribute>();
            var names = new List<string>();
            if (SchemaAttribute != null)
            {
                names.Add(SchemaAttribute.Name);
            }

            GetParents(this, parents);
            parents.Reverse();
            var result = parents.Where(p => p.SchemaAttribute != null).Select(p => p.SchemaAttribute.Name).ToList();
            result.AddRange(names);
            return string.Join(".", result);
        }

        private IEnumerable<RepresentationAttribute> GetParents(RepresentationAttribute representation, ICollection<RepresentationAttribute> parents)
        {
            if (representation.Parent == null)
            {
                return parents;
            }

            parents.Add(representation.Parent);
            return GetParents(representation.Parent, parents);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (!(obj is RepresentationAttribute representation))
            {
                return 1;
            }

            return CompareTo(representation);
        }

        public virtual bool SetValue(RepresentationAttribute attr)
        {
            return false;
        }

        protected virtual int CompareTo(RepresentationAttribute attr)
        {
            return 0;
        }
    }
}
