namespace SimpleIdentityServer.Shared.DTOs
{
    using System.Runtime.Serialization;

    [DataContract]
    public class Address
    {
        [DataMember(Name = ScimConstants.AddressResponseNames.Formatted)]
        public string Formatted { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.StreetAddress)]
        public string StreetAddress { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Locality)]
        public string Locality { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Region)]
        public string Region { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.PostalCode)]
        public string PostalCode { get; set; }
        [DataMember(Name = ScimConstants.AddressResponseNames.Country)]
        public string Country { get; set; }
    }
}