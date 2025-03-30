namespace DotAuth.Tests.Validators;

using System.Text.Json;
using DotAuth.Shared;

internal static class ObjectExtensions
{
    public static string SerializeWithJavascript(this object parameter)
    {
        return JsonSerializer.Serialize(parameter, DefaultJsonSerializerOptions.Instance);
    }

    public static T DeserializeWithJavascript<T>(this string parameter)
    {
        return JsonSerializer.Deserialize<T>(parameter, DefaultJsonSerializerOptions.Instance)!;
    }
}
