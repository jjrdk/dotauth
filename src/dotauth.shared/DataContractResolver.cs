// Code copied from https://github.com/zcsizmadia/ZCS.DataContractResolver

namespace DotAuth.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;

internal class DataContractResolver : IJsonTypeInfoResolver
{
    private static readonly Dictionary<Type, TypeMembers[]> Infos = new();
    private static DataContractResolver? _defaultInstance;

    public static DataContractResolver Default
    {
        get
        {
            if (_defaultInstance is { } result)
            {
                return result;
            }

            DataContractResolver newInstance = new();
            var originalInstance =
                Interlocked.CompareExchange(ref _defaultInstance, newInstance, comparand: null);
            return originalInstance ?? newInstance;
        }
    }

    private static bool IsNullOrDefault(object? obj)
    {
        if (obj is null)
        {
            return true;
        }

        var type = obj.GetType();

        return type.IsValueType && FormatterServices.GetUninitializedObject(type).Equals(obj);
    }

    private static IEnumerable<MemberInfo> EnumerateFieldsAndProperties(Type type, BindingFlags bindingFlags)
    {
        foreach (var fieldInfo in type.GetFields(bindingFlags))
        {
            yield return fieldInfo;
        }

        foreach (var propertyInfo in type.GetProperties(bindingFlags))
        {
            yield return propertyInfo;
        }
    }

    private static IEnumerable<JsonPropertyInfo> CreateDataMembers(JsonTypeInfo jsonTypeInfo)
    {
        TypeMembers[] GetTypeMembers(Type type)
        {
            var isDataContract = type.GetCustomAttribute<DataContractAttribute>() != null;
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;

            if (isDataContract)
            {
                bindingFlags |= BindingFlags.NonPublic;
            }

            return EnumerateFieldsAndProperties(type, bindingFlags)
                .Select(memberInfo => GetInfo(isDataContract, memberInfo))
                .Where(m => m != null)
                .Select(m => m!)
                .ToArray();
        }

        if (!Infos.ContainsKey(jsonTypeInfo.Type))
        {
            Infos[jsonTypeInfo.Type] = GetTypeMembers(jsonTypeInfo.Type);
        }

        var members = Infos[jsonTypeInfo.Type];
        return members.Select(typeMembers =>
        {
            var jsonPropertyInfo =
                jsonTypeInfo.CreateJsonPropertyInfo(typeMembers.PropertyType, typeMembers.PropertyName);

            jsonPropertyInfo.Get = typeMembers.GetValue;
            jsonPropertyInfo.Set = typeMembers.SetValue;

            if (typeMembers.Order != null)
            {
                jsonPropertyInfo.Order = typeMembers.Order.Value;
                jsonPropertyInfo.ShouldSerialize =
                    !typeMembers.EmitDefaultValue ? (_, obj) => !IsNullOrDefault(obj) : null;
            }

            return jsonPropertyInfo;
        });
    }

    private static bool IgnoreMember(
        MemberInfo memberInfo,
        bool isDataContract,
        ref DataMemberAttribute? dataMemberAttribute)
    {
        if (isDataContract)
        {
            dataMemberAttribute = memberInfo.GetCustomAttribute<DataMemberAttribute>();
            if (dataMemberAttribute == null)
            {
                return true;
            }
        }

        return memberInfo.GetCustomAttribute<IgnoreDataMemberAttribute>() != null;
    }

    private static TypeMembers? GetInfo(
        bool isDataContract,
        MemberInfo memberInfo)
    {
        DataMemberAttribute? attr = null;

        if (IgnoreMember(memberInfo, isDataContract, ref attr))
        {
            return null;
        }

        Func<object, object?>? getValue = null;
        Action<object, object?>? setValue = null;
        Type? propertyType;
        string? propertyName;

        if (memberInfo.MemberType == MemberTypes.Field && memberInfo is FieldInfo fieldInfo)
        {
            propertyName = attr?.Name ?? fieldInfo.Name;
            propertyType = fieldInfo.FieldType;
            getValue = fieldInfo.GetValue;
            setValue = (obj, value) => fieldInfo.SetValue(obj, value);
        }
        else if (memberInfo.MemberType == MemberTypes.Property && memberInfo is PropertyInfo propertyInfo)
        {
            propertyName = attr?.Name ?? propertyInfo.Name;
            propertyType = propertyInfo.PropertyType;
            if (propertyInfo.CanRead)
            {
                getValue = propertyInfo.GetValue;
            }

            if (propertyInfo.CanWrite)
            {
                setValue = (obj, value) => propertyInfo.SetValue(obj, value);
            }
        }
        else
        {
            return null;
        }

        var members = new TypeMembers
        {
            GetValue = getValue,
            Order = attr?.Order,
            EmitDefaultValue = attr?.EmitDefaultValue ?? false,
            PropertyType = propertyType,
            SetValue = setValue,
            PropertyName = propertyName
        };

        return members;
    }

    private static JsonTypeInfo GetTypeInfo(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind == JsonTypeInfoKind.Object)
        {
            jsonTypeInfo.CreateObject = () => Activator.CreateInstance(jsonTypeInfo.Type)!;

            foreach (var jsonPropertyInfo in CreateDataMembers(jsonTypeInfo).OrderBy((x) => x.Order))
            {
                jsonTypeInfo.Properties.Add(jsonPropertyInfo);
            }
        }

        return jsonTypeInfo;
    }

    public JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var jsonTypeInfo = JsonTypeInfo.CreateJsonTypeInfo(type, options);
        return GetTypeInfo(jsonTypeInfo);
    }
}
