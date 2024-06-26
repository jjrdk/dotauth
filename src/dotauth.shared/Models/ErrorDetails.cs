﻿namespace DotAuth.Shared.Models;

using System.Net;
using System.Runtime.Serialization;

///// <summary>
///// Defines the OAuth error type.
///// </summary>
//[DataContract]
//internal sealed class OauthError
//{
//    /// <summary>
//    /// Gets or sets the error.
//    /// </summary>
//    [DataMember] public string Error { get; set; } = null!;
//}

/// <summary>
/// A machine-readable format for specifying errors in HTTP API responses based on https://tools.ietf.org/html/rfc7807.
/// </summary>
[DataContract]
public sealed record ErrorDetails
{
    /// <summary>
    /// A short, human-readable summary of the problem type.It SHOULD NOT change from occurrence to occurrence
    /// of the problem, except for purposes of localization(e.g., using proactive content negotiation;
    /// see[RFC7231], Section 3.4).
    /// </summary>
    [DataMember(Name = "error")]
    public string Title { get; set; } = null!;

    /// <summary>
    /// The HTTP status code([RFC7231], Section 6) generated by the origin server for this occurrence of the problem.
    /// </summary>
    [DataMember(Name = "status", IsRequired = false)]
    public HttpStatusCode Status { get; set; } = HttpStatusCode.BadRequest;

    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    [DataMember(Name = "detail", IsRequired = false)]
    public string Detail { get; set; } = null!;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Title} - {Detail.Replace('\n', ' ').Replace("\r", string.Empty)} - {Status}";
    }
}