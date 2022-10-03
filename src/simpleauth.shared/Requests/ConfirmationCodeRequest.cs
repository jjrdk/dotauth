﻿namespace DotAuth.Shared.Requests;

using System.Runtime.Serialization;

/// <summary>
/// Defines the confirmation code request.
/// </summary>
[DataContract]
public sealed record ConfirmationCodeRequest
{
    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    /// <value>
    /// The phone number.
    /// </value>
    [DataMember(Name = "phone_number")]
    public string? PhoneNumber { get; set; }
}