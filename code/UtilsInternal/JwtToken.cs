// <copyright file="JwtToken.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Principal
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// <c>Jwt</c> token used by Social Plus
    /// </summary>
    public class JwtToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtToken"/> class
        /// </summary>
        /// <param name="algorithm"><c>jwt</c> signing algorithm</param>
        /// <param name="claims"><c>jwt</c> claims</param>
        public JwtToken(JwtAlgorithm algorithm, JwtClaims claims)
        {
            this.Algorithm = algorithm;
            this.Claims = claims;
            this.InitializeHeaderAndPayload();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtToken"/> class
        /// </summary>
        /// <param name="token"><c>Jwt</c> token</param>
        public JwtToken(string token)
        {
            this.Token = token;
            this.ParseToken();
        }

        /// <summary>
        /// Gets or sets <c>Jwt</c> signing algorithm
        /// </summary>
        public JwtAlgorithm Algorithm { get; set; }

        /// <summary>
        /// Gets or sets <c>Jwt</c> claims
        /// </summary>
        public JwtClaims Claims { get; set; }

        /// <summary>
        /// Gets or sets header and payload concatenated string
        /// </summary>
        public string HeaderAndPayload { get; set; }

        /// <summary>
        /// Gets or sets signature byte array in the <c>jwt</c> token
        /// </summary>
        public byte[] Signature { get; set; }

        /// <summary>
        /// Gets or sets <c>Jwt</c> token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Get header and payload data to sign
        /// </summary>
        /// <returns>byte array of concatenated header and payload based on <c>jwt</c> specifications</returns>
        public byte[] GetDataToSign()
        {
            var bytesToSign = Encoding.UTF8.GetBytes(this.HeaderAndPayload);
            return bytesToSign;
        }

        /// <summary>
        /// Generate token from signature and the initialized claims
        /// </summary>
        /// <param name="signature">Signed header and payload</param>
        /// <returns><c>jwt</c> token</returns>
        public string GenerateToken(byte[] signature)
        {
            this.Signature = signature;
            string base64Signature = Base64Url.Encode(signature);
            this.Token = string.Join(".", this.HeaderAndPayload, base64Signature);
            return this.Token;
        }

        /// <summary>
        /// Initialize header and payload string from algorithm and claims
        /// </summary>
        private void InitializeHeaderAndPayload()
        {
            var header = new { alg = this.Algorithm.ToString(), typ = "JWT" };
            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this.Claims, Formatting.None));
            string base64Header = Base64Url.Encode(headerBytes);
            string base64Payload = Base64Url.Encode(payloadBytes);
            var stringToSign = string.Join(".", base64Header, base64Payload);
            this.HeaderAndPayload = stringToSign;
        }

        /// <summary>
        /// Parse <c>jwt</c> token to extract algorithm, claims, and signature
        /// </summary>
        private void ParseToken()
        {
            var parts = this.Token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            this.Signature = Base64Url.Decode(parts[2]);
            this.HeaderAndPayload = string.Join(".", header, payload);

            var headerJson = Encoding.UTF8.GetString(Base64Url.Decode(header));
            var payloadJson = Encoding.UTF8.GetString(Base64Url.Decode(payload));
            this.Algorithm = (JwtAlgorithm)Enum.Parse(typeof(JwtAlgorithm), (string)JObject.Parse(headerJson)["alg"]);
            this.Claims = JsonConvert.DeserializeObject<JwtClaims>(payloadJson);
        }
    }
}
