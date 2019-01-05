//namespace SimpleAuth.Uma.Controllers
//{
//    using Microsoft.AspNetCore.Mvc;
//    using Microsoft.IdentityModel.Tokens;
//    using SimpleAuth.Shared.Repositories;
//    using System.Threading.Tasks;

//    [Route(UmaConstants.RouteValues.Jwks)]
//    public class JwksController : Controller
//    {
//        private readonly IJsonWebKeyRepository _repository;

//        public JwksController(IJsonWebKeyRepository repository)
//        {
//            _repository = repository;
//        }

//        [HttpGet]
//        public async Task<JsonWebKeySet> Get()
//        {
//            return await _repository.GetAllAsync().ConfigureAwait(false);
//        }

//        //[HttpPut]
//        //public async Task<bool> Put()
//        //{
//        //    return await _jwksActions.RotateJwks().ConfigureAwait(false);
//        //}
//    }
//}
