﻿namespace DotAuth.Shared.Models;

/// <summary>
/// Defines the scope types.
/// </summary>
public static class ScopeTypes
{
    /// <summary>
    /// The protected API scope type.
    /// </summary>
    public static string ProtectedApi
    {
        get { return "ProtectedApi"; }
    }

    /// <summary>
    /// The resource owner scope type.
    /// </summary>
    public static string ResourceOwner
    {
        get { return "ResourceOwner"; }
    }
}