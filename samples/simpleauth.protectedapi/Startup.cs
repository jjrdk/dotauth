// Copyright © 2015 Habart Thierry, © 2018 Jacob Reimers
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SimpleAuth.ProtectedApi
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using Shared;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => options.AddPolicy(
                "AllowAll",
                p => p.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()));
            services.AddLogging();
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                "mycert.pfx");
            var certificate = new X509Certificate2(
                path,
                "simpleauth",
                X509KeyStorageFlags.Exportable);
            var key = certificate.CreateJwk(
                JsonWebKeyUseNames.Sig,
                KeyOperations.Sign,
                KeyOperations.Verify);

            services.AddAuthentication(cfg =>
                {
                    cfg.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                    cfg =>
                    {
                        cfg.Authority = "https://localhost";
                        cfg.Audience = "api";
                        cfg.MetadataAddress = "https://localhost/.well-known/openid-configuration";
                        cfg.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidIssuer = "https://localhost",
                            ValidateAudience = true,
                            ValidAudience = "api",
                            ValidateIssuerSigningKey = false,
                            IssuerSigningKey = key
                        };
                    });

            services.AddMvc();
        }

        public void Configure(
            IApplicationBuilder app,
            ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            app.UseAuthentication();
            //1 . Enable CORS.
            app.UseCors("AllowAll");
            // 5. Configure ASP.NET MVC
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
    public static class JsonWebKeyExtensions
    {
        public static JsonWebKey CreateJwk(this X509Certificate2 certificate, string use, params string[] keyOperations)
        {
            if (keyOperations == null)
            {
                throw new ArgumentNullException(nameof(keyOperations));
            }
            JsonWebKey jwk = null;
            if (certificate.HasPrivateKey)
            {
                var keyAlg = certificate.SignatureAlgorithm.FriendlyName;
                if (keyAlg.Contains("RSA"))
                {
                    var rsa = (RSA)certificate.PrivateKey;
                    var parameters = rsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Kid = certificate.Thumbprint,
                        Kty = JsonWebAlgorithmsKeyTypes.RSA,
                        Alg = keyAlg,
                        E = parameters.Exponent == null ? null : Convert.ToBase64String(parameters.Exponent),
                        N = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Modulus),
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        DP = parameters.DP == null ? null : Convert.ToBase64String(parameters.DP),
                        DQ = parameters.DQ == null ? null : Convert.ToBase64String(parameters.DQ),
                        QI = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.InverseQ),
                        P = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.P),
                        Q = parameters.Modulus == null ? null : Convert.ToBase64String(parameters.Q)
                    };
                }
                else if (keyAlg.Contains("ecdsa"))
                {
                    var ecdsa = certificate.GetECDsaPrivateKey();
                    var parameters = ecdsa.ExportParameters(true);
                    jwk = new JsonWebKey
                    {
                        Kty = JsonWebAlgorithmsKeyTypes.EllipticCurve,
                        Alg = keyAlg,
                        D = parameters.D == null ? null : Convert.ToBase64String(parameters.D),
                        Crv = parameters.Curve.Hash.ToString(),
                        X = parameters.Q.X.ToBase64Simplified(),
                        Y = parameters.Q.Y.ToBase64Simplified()
                        //Q = parameters.Q == null ? null:Convert.ToBase64String(parameters.Q),
                    };
                }
            }

            if (jwk == null)
            {
                jwk = JsonWebKeyConverter.ConvertFromX509SecurityKey(new X509SecurityKey(certificate));
            }

            jwk.Use = use;
            jwk.X5t = certificate.Thumbprint;
            jwk.Kid = certificate.Thumbprint;

            foreach (var keyOperation in keyOperations)
            {
                jwk.KeyOps.Add(keyOperation);
            }

            return jwk;
        }
    }
}
