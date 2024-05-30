namespace DotAuth.Uma;

using System;
using System.Runtime.Serialization;

public class RegistrationData : ResourceRegistration
{
    /// <summary>
    /// Gets or sets the content description
    /// </summary>
    [DataMember(Name = "files")]
    public FileDescription[] Data { get; set; } = [];

    public override RegistrationData ToRegistrationData()
    {
        return this;
    }
}