namespace SimpleAuth.Results
{
    /// <summary>
    /// Redirection instruction parameter.
    /// </summary>
    public class Parameter
    {
        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; }

        /// <summary>Gets or sets the value.</summary>
        public string Value { get; }
    }
}