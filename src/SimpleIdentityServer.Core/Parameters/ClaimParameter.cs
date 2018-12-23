namespace SimpleIdentityServer.Core.Parameters
{
    using System.Collections.Generic;
    using System.Linq;

    public class ClaimParameter
    {
        public string Name { get; set; }

        public Dictionary<string, object> Parameters { get; set; }

        public bool Essential => GetBoolean(CoreConstants.StandardClaimParameterValueNames.EssentialName);

        public string Value => GetString(CoreConstants.StandardClaimParameterValueNames.ValueName);

        public string[] Values => GetArray(CoreConstants.StandardClaimParameterValueNames.ValuesName);

        public bool EssentialParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == CoreConstants.StandardClaimParameterValueNames.EssentialName);
            }
        }

        public bool ValueParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == CoreConstants.StandardClaimParameterValueNames.ValueName);
            }
        }

        public bool ValuesParameterExist
        {
            get
            {
                return Parameters.Any(p => p.Key == CoreConstants.StandardClaimParameterValueNames.ValuesName);
            }
        }

        private bool GetBoolean(string name)
        {
            var value = GetString(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return bool.TryParse(value, out var result) && result;
        }

        private string GetString(string name)
        {
            var keyPair = Parameters.FirstOrDefault(p => p.Key == name);
            if (keyPair.Equals(default(KeyValuePair<string, object>))
                || string.IsNullOrWhiteSpace(keyPair.ToString()))
            {
                return string.Empty;
            }

            return keyPair.Value.ToString();
        }

        private string[] GetArray(string name)
        {
            var keyPair = Parameters.FirstOrDefault(p => p.Key == name);
            if (keyPair.Equals(default(KeyValuePair<string, object>))
                || string.IsNullOrWhiteSpace(keyPair.ToString()))
            {
                return null;
            }

            var value = keyPair.Value;
            if (!value.GetType().IsArray)
            {
                return null;
            }

            var result = (object[])value;
            return result.Select(r => r.ToString()).ToArray();
        }
    }
}