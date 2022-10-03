namespace DotAuth.Tests.Validators;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

internal static class ObjectExtensions
{
    private static readonly JsonConverter[] Converters = { new StringEnumConverter() };

    public static string SerializeWithJavascript(this object parameter)
    {
        return JsonConvert.SerializeObject(parameter, Converters);
    }

    public static T DeserializeWithJavascript<T>(this string parameter)
    {
        return JsonConvert.DeserializeObject<T>(parameter, Converters);
    }
}