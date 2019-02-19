namespace SimpleAuth.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    public class ProfileViewModel
    {
        public ProfileViewModel(Claim[] claims)
        {
            Name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value ?? "Unknown";
            GivenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName || c.Type == "given_name")?.Value
                        ?? " - ";
            FamilyName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname || c.Type == "family_name")?.Value
                        ?? " - ";
            Picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            LinkedIdentityProviders = new List<IdentityProviderViewModel>();
            UnlinkedIdentityProviders = new List<IdentityProviderViewModel>();
        }

        /*
         var nameClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name");
    var givenNameClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName || c.Type == "given_name");
    var familyNameClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname || c.Type == "family_name");
    var pictureClaim = identity.Claims.FirstOrDefault(c => c.Type == "picture");
    //var roles = identity.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
    var name = nameClaim == null ? "Unknown" : nameClaim.Value;
    var givenName = givenNameClaim == null ? " - " : givenNameClaim.Value;
    var familyName = familyNameClaim == null ? " - " : familyNameClaim.Value;
    var picture = pictureClaim == null ? Url.Content("~/img/unknown.png") : pictureClaim.Value;
         */
        public string Name { get; }

        public string GivenName { get; }

        public string FamilyName { get; }

        public string Picture { get; }

        public ICollection<IdentityProviderViewModel> LinkedIdentityProviders { get; set; }
        public ICollection<IdentityProviderViewModel> UnlinkedIdentityProviders { get; set; }
    }
}
