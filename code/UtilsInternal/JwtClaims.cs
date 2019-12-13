// <copyright file="JwtClaims.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Principal
{
    using System;
    using System.Runtime.Serialization;
    using SocialPlus.Utils;

    /// <summary>
    /// <c>Jwt</c> claims used by Social Plus.
    /// </summary>
    [DataContract]
    public class JwtClaims
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtClaims"/> class
        /// </summary>
        /// <param name="userHandle">User handle</param>
        /// <param name="appKey">App key</param>
        /// <param name="validFor">Time interval representing how long the token should be valid for.</param>
        public JwtClaims(string userHandle, string appKey, TimeSpan validFor)
        {
            this.UserHandle = userHandle;
            this.AppKey = appKey;
            TimeSpan expirationUnixTime = TimeUtils.DateTime2UnixTime(DateTime.UtcNow.Add(validFor));
            this.ExpirationUnixTime = (long)expirationUnixTime.TotalSeconds;
        }

        /// <summary>
        /// Gets a value indicating whether the token has expired
        /// </summary>
        public bool HasExpired
        {
            get
            {
                TimeSpan nowUnixTime = TimeUtils.DateTime2UnixTime(DateTime.UtcNow);
                return this.ExpirationUnixTime < (long)nowUnixTime.TotalSeconds;
            }
        }

        /// <summary>
        /// Gets or sets user handle
        /// </summary>
        [DataMember(Name = "sub")]
        public string UserHandle { get; set; }

        /// <summary>
        /// Gets or sets app key
        /// </summary>
        [DataMember(Name = "app")]
        public string AppKey { get; set; }

        /// <summary>
        /// Gets or sets expiration time in seconds (unix time format)
        /// </summary>
        [DataMember(Name = "exp")]
        private long ExpirationUnixTime { get; set; }
    }
}
