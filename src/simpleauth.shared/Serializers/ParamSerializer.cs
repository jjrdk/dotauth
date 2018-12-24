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

namespace SimpleIdentityServer.Shared.Serializers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal class SharedErrorDescriptions{
        public const string TheRedirectionUriIsNotWellFormed = "Based on the RFC-3986 the redirection-uri is not well formed";
    }

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

        public T Deserialize<T>(IEnumerable<KeyValuePair<string, string[]>> keyValues)
        {
            var dict = keyValues.ToDictionary(x => x.Key, x => x.Value);
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

                        object value = stringValue;
                        if (typeof(Uri) == type)
                        {
                            try
                            {
                                value = new Uri(stringValue.ToString());
                            }
                            catch (Exception e)
                            {
                                throw new Exception(
                                    SharedErrorDescriptions.TheRedirectionUriIsNotWellFormed,
                                    e);
                            }
                        }
                        else if (type.IsEnum)
                        {
                            value = Enum.Parse(type, (string) stringValue, true);
                        }

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
                input.AllKeys.Select(x => new KeyValuePair<string, string[]>(x, new[] { input[x] }));
            return Deserialize<T>(keyvalues);
        }
    }
}
