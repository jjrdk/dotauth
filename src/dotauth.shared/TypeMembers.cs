namespace DotAuth.Shared;

using System;

internal record TypeMembers
{
    public int? Order { get; init; }
    public bool EmitDefaultValue { get; init; }
    public Func<object, object?>? GetValue { get; init; }
    public Action<object, object?>? SetValue { get; init; }
    public Type PropertyType { get; init; } = null!;
    public string PropertyName { get; init; } = null!;
}
