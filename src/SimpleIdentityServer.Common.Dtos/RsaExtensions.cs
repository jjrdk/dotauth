// Copyright 2015 Habart Thierry
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

namespace SimpleIdentityServer.Shared
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public static class RsaExtensions
    {
        public static string ToXmlString(this RSA rsa, bool includePrivateParameters)
        {
            // From the XMLDSIG spec, RFC 3075, Section 6.4.2,
            RSAParameters rsaParams = rsa.ExportParameters(includePrivateParameters);
            var builder = new StringBuilder();
            builder.Append("<RSAKeyValue>");
            // Add the modulus
            builder.Append("<Modulus>" + Convert.ToBase64String(rsaParams.Modulus) + "</Modulus>");
            // Add the exponent
            builder.Append("<Exponent>" + Convert.ToBase64String(rsaParams.Exponent) + "</Exponent>");
            if (includePrivateParameters)
            {
                // Add the private components 
                builder.Append("<P>" + Convert.ToBase64String(rsaParams.P) + "</P>");
                builder.Append("<Q>" + Convert.ToBase64String(rsaParams.Q) + "</Q>");
                builder.Append("<DP>" + Convert.ToBase64String(rsaParams.DP) + "</DP>");
                builder.Append("<DQ>" + Convert.ToBase64String(rsaParams.DQ) + "</DQ>");
                builder.Append("<InverseQ>" + Convert.ToBase64String(rsaParams.InverseQ) + "</InverseQ>");
                builder.Append("<D>" + Convert.ToBase64String(rsaParams.D) + "</D>");
            }

            builder.Append("</RSAKeyValue>");
            return builder.ToString();
        }
    }
}
