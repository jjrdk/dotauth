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

namespace SimpleAuth.Shared
{
    using System;

    /// <summary>
    /// Definition of a JSON Web Key (JWK)
    /// It's a JSON data structure that represents a cryptographic key
    /// </summary>
    public class JsonWebKey
    {
        /// <summary>
        /// Gets or sets the cryptographic algorithm family used with the key.
        /// </summary>
        public KeyType Kty { get; set; }

        /// <summary>
        /// Gets or sets the intended use of the public key.
        /// Employed to indicate whether a public key is used for encrypting data or verifying the signature on data.
        /// </summary>
        public Use Use { get; set; }

        /// <summary>
        /// Gets or sets the operation(s) that the key is intended to be user for.
        /// </summary>
        public KeyOperations[] KeyOps { get; set; }

        /// <summary>
        /// Gets or sets the algorithm intended for use with the key
        /// </summary>
        public AllAlg Alg { get; set; }

        /// <summary>
        /// Gets or sets the KID (key id). 
        /// </summary>
        public string Kid { get; set; }

        /// <summary>
        /// Gets or sets the X5U. It's a URI that refers to a resource for an X509 public key certificate or certificate chain.
        /// </summary>
        public Uri X5u { get; set; }

        /// <summary>
        /// Gets or sets the X5T. Is a base64url encoded SHA-1 thumbprint of the DER encoding of an X509 certificate.
        /// </summary>
        public string X5t { get; set; }

        /// <summary>
        /// Gets or sets the X5T#S256. Is a base64url encoded SHA-256 thumbprint.
        /// </summary>
        public string X5tS256 { get; set; }

        /// <summary>
        /// Gets or sets the serialized key in XML
        /// </summary>
        public string SerializedKey { get; set; }
    }
}
