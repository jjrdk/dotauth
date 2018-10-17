﻿namespace SimpleIdentityServer.Core.Common.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class EnrichedErrorResponse : ScimErrorResponse
    {
        [DataMember(Name = ScimConstants.EnrichedErrorResponseNames.ScimType)]
        public string ScimType { get; set; }
    }
}