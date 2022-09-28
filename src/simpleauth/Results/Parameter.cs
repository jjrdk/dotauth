namespace SimpleAuth.Results;

/// <summary>
/// Redirection instruction parameter.
/// </summary>
public sealed record Parameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Parameter"/> class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
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