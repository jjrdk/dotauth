namespace SimpleAuth.Extensions
{
    public static class StructExtensions
    {
        public static bool IsDefault<T>(this T value) where T : struct
        {
            var isDefault = value.Equals(default(T));
            return isDefault;
        }
    }
}