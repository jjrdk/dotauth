//namespace SimpleAuth.Repositories
//{
//    using Shared.Parameters;
//    using Shared.Repositories;
//    using Shared.Results;
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Security.Claims;
//    using System.Threading.Tasks;

//    internal sealed class DefaultClaimRepository : IClaimRepository
//    {
//        private readonly ICollection<string> _claims;

//        private readonly List<string> DEFAULT_CLAIMS = new List<string>
//        {
//             JwtConstants.StandardResourceOwnerClaimNames.Subject ,
//             JwtConstants.StandardResourceOwnerClaimNames.Name ,
//             JwtConstants.StandardResourceOwnerClaimNames.FamilyName ,
//             JwtConstants.StandardResourceOwnerClaimNames.GivenName ,
//             JwtConstants.StandardResourceOwnerClaimNames.MiddleName ,
//             JwtConstants.StandardResourceOwnerClaimNames.NickName ,
//             JwtConstants.StandardResourceOwnerClaimNames.PreferredUserName ,
//             JwtConstants.StandardResourceOwnerClaimNames.Profile ,
//             JwtConstants.StandardResourceOwnerClaimNames.Picture ,
//             JwtConstants.StandardResourceOwnerClaimNames.WebSite ,
//             JwtConstants.StandardResourceOwnerClaimNames.Gender ,
//             JwtConstants.StandardResourceOwnerClaimNames.BirthDate ,
//             JwtConstants.StandardResourceOwnerClaimNames.ZoneInfo ,
//             JwtConstants.StandardResourceOwnerClaimNames.Locale ,
//             JwtConstants.StandardResourceOwnerClaimNames.UpdatedAt ,
//             JwtConstants.StandardResourceOwnerClaimNames.Email ,
//             JwtConstants.StandardResourceOwnerClaimNames.EmailVerified ,
//             JwtConstants.StandardResourceOwnerClaimNames.Address ,
//             JwtConstants.StandardResourceOwnerClaimNames.PhoneNumber ,
//             JwtConstants.StandardResourceOwnerClaimNames.PhoneNumberVerified ,
//             JwtConstants.StandardResourceOwnerClaimNames.Role ,
//             JwtConstants.StandardResourceOwnerClaimNames.ScimId ,
//             JwtConstants.StandardResourceOwnerClaimNames.ScimLocation
//        };

//        public DefaultClaimRepository(IReadOnlyCollection<string> claims = null)
//        {
//            _claims = (claims == null || claims.Count == 0)
//            ? _claims = DEFAULT_CLAIMS
//            : _claims = claims.ToList();
//        }

//        public Task<bool> Delete(string code)
//        {
//            if (string.IsNullOrWhiteSpace(code))
//            {
//                throw new ArgumentNullException(nameof(code));
//            }

//            var claim = _claims.FirstOrDefault(c => c == code);
//            if (claim == null)
//            {
//                return Task.FromResult(false);
//            }

//            _claims.Remove(claim);
//            return Task.FromResult(true);
//        }

//        public Task<IEnumerable<string>> GetAllAsync()
//        {
//            return Task.FromResult(_claims.Select(c => c));
//        }

//        public Task<bool> GetAsync(string name)
//        {
//            if (string.IsNullOrWhiteSpace(name))
//            {
//                // throw new ArgumentNullException(nameof(name));
//                return Task.FromResult(false);
//            }

//            var res = _claims.Any(c => c == name);

//            return Task.FromResult(res);
//        }

//        public Task<bool> InsertAsync(string claim)
//        {
//            if (claim == null)
//            {
//                throw new ArgumentNullException(nameof(claim));
//            }

//            _claims.Add(claim);
//            return Task.FromResult(true);
//        }

//        public Task<SearchClaimsResult> Search(SearchClaimsParameter parameter)
//        {
//            if (parameter == null)
//            {
//                throw new ArgumentNullException(nameof(parameter));
//            }

//            IEnumerable<Claim> result = _claims;
//            if (parameter.ClaimKeys != null)
//            {
//                result = result.Where(c => parameter.ClaimKeys.Any(ck => c.Code.Contains(ck)));
//            }

//            var nbResult = result.Count();
//            if (parameter.Order != null)
//            {
//                switch (parameter.Order.Target)
//                {
//                    case "update_datetime":
//                        switch (parameter.Order.Type)
//                        {
//                            case OrderTypes.Asc:
//                                result = result.OrderBy(c => c.UpdateDateTime);
//                                break;
//                            case OrderTypes.Desc:
//                                result = result.OrderByDescending(c => c.UpdateDateTime);
//                                break;
//                        }
//                        break;
//                }
//            }
//            else
//            {
//                result = result.OrderByDescending(c => c.UpdateDateTime);
//            }

//            if (parameter.IsPagingEnabled)
//            {
//                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
//            }

//            return Task.FromResult(new SearchClaimsResult
//            {
//                Content = result.Select(c => c.Copy()),
//                StartIndex = parameter.StartIndex,
//                TotalResults = nbResult
//            });
//        }

//        public Task<bool> Update(Claim claim)
//        {
//            if (claim == null)
//            {
//                throw new ArgumentNullException(nameof(claim));
//            }

//            var result = _claims.FirstOrDefault(c => c.Code == claim.Code);
//            if (result == null)
//            {
//                return Task.FromResult(false);
//            }

//            result.Value = claim.Value;
//            result.UpdateDateTime = claim.UpdateDateTime;
//            return Task.FromResult(true);
//        }
//    }
//}
