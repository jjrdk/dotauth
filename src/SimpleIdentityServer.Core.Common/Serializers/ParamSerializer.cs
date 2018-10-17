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

using System;
using System.Collections.Specialized;
using System.Linq;

namespace SimpleIdentityServer.Core.Common.Serializers
{
    using Microsoft.Extensions.Primitives;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    public class ParamSerializer
    {
        public string Serialize(object obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();
            var keyvalues =
                properties.Select(p => new KeyValuePair<string, object>(GetPropertyAlias(p), p.GetValue(obj)))
                    .Where(x => x.Value != null);
            return string.Join("&", keyvalues.Select(x => $"{x.Key}={x.Value}"));
        }

        public T Deserialize<T>(IEnumerable<KeyValuePair<string, StringValues>> keyValues)
        {
            var dict = keyValues.ToDictionary(x => x.Key, x => x.Value.ToArray());
            var properties = typeof(T).GetProperties();

            var instance = Activator.CreateInstance<T>();
            foreach (var property in properties)
            {
                var name = GetPropertyAlias(property);
                if (dict.TryGetValue(name, out var values))
                {
                    if (values.Length > 0)
                    {
                        var stringValue = values.Length == 1
                            ? (object)values[0]
                            : values;
                        var type = property.PropertyType;
                        if (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
                        {
                            type = type.GetGenericArguments()[0];
                        }

                        var value = type.IsEnum
                            ? Enum.Parse(type, (string)stringValue, true)
                            : stringValue;


                        property.SetValue(
                            instance,
                            Convert.ChangeType(
                                value,
                                type));
                    }
                }
            }

            return instance;
        }

        private static string GetPropertyAlias(PropertyInfo property)
        {
            var datamember = property.GetCustomAttribute<DataMemberAttribute>(true);
            var name = datamember?.Name ?? property.Name;
            return name;
        }

        public T Deserialize<T>(NameValueCollection input)
        {
            var keyvalues =
                input.AllKeys.Select(x => new KeyValuePair<string, StringValues>(x, new StringValues(input[x])));
            return Deserialize<T>(keyvalues);
        }
    }
}
