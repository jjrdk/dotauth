namespace SimpleAuth.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using SimpleAuth.Shared.Repositories;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the JwksController.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route(CoreConstants.EndPoints.Jwks)]
    public class JwksController : ControllerBase
    {
        private readonly IJwksRepository _jwksStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwksController"/> class.
        /// </summary>
        /// <param name="jwksStore">The JWKS store.</param>
        public JwksController(IJwksRepository jwksStore)
        {
            _jwksStore = jwksStore;
        }

        /// <summary>
        /// Gets the <see cref="JsonWebKeySet"/>.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 300)]
        public async Task<ActionResult<JsonWebKeySet>> Get(CancellationToken cancellationToken)
        {
            var jwks = await _jwksStore.GetPublicKeys(cancellationToken).ConfigureAwait(false);

            return jwks;
        }

        /// <summary>
        /// Adds the specified json web key.
        /// </summary>
        /// <param name="jsonWebKey">The json web key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize("manager")]
        public async Task<IActionResult> Add(JsonWebKey jsonWebKey, CancellationToken cancellationToken)
        {
            var result = await _jwksStore.Add(jsonWebKey, cancellationToken).ConfigureAwait(false);

            return result ? Ok() : (IActionResult)BadRequest();
        }

        /// <summary>
        /// Rotates the specified json web key set.
        /// </summary>
        /// <param name="jsonWebKeySet">The json web key set.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpPut]
        [Authorize("manager")]
        public async Task<IActionResult> Rotate(JsonWebKeySet jsonWebKeySet, CancellationToken cancellationToken)
        {
            var result = await _jwksStore.Rotate(jsonWebKeySet, cancellationToken).ConfigureAwait(false);

            return result ? Ok() : (IActionResult)BadRequest();
        }
    }
}