// <copyright file="UserPrincipal.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Principal
{
    using System;
    using System.Security.Principal;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Utils;

    /// <summary>
    /// Default implementation of IUserPrincipal. A user principal is the authenticated entity that corresponds
    /// to a user making a request. It can consist of a "valid" user handle, an identity provider and its accountid.
    /// Valid is in quotes because we cannot guarantee that the user handle was not
    /// deleted in between the authentication filter's check and the controller executing.
    /// Note that in some cases (e.g., calling PostUser), the user handle is null, but the identity provider and its account id are valid.
    /// </summary>
    public class UserPrincipal : IPrincipal
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPrincipal"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="userHandle">User handle</param>
        /// <param name="identityProvider">the type of the identity provider</param>
        /// <param name="identityProviderAccountId">this user's account id for the identity provider</param>
        public UserPrincipal(ILog log, string userHandle, IdentityProviderType identityProvider, string identityProviderAccountId)
        {
            this.log = log;
            this.Setup(userHandle, identityProvider, identityProviderAccountId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPrincipal"/> class
        /// </summary>
        /// <param name="serializedUserPrincipal">the user principal in a serialized form</param>
        public UserPrincipal(string serializedUserPrincipal)
        {
            // Call internal util method split on delimeter.
            string[] splitOnNewline = serializedUserPrincipal.Split('\n');
            if (splitOnNewline.Length != 3)
            {
                this.log.LogException("The serializedUserPrincipal is malformed.");
            }

            IdentityProviderType identityProviderType = (IdentityProviderType)Enum.Parse(typeof(IdentityProviderType), splitOnNewline[1]);
            this.Setup(splitOnNewline[0], identityProviderType, splitOnNewline[2]);
        }

        /// <summary>
        /// Gets identity for principal. Must be implemented because it is required by the IPrincipal interface,
        /// but it is unused by our service
        /// </summary>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// Gets user handle
        /// </summary>
        public string UserHandle { get; private set; }

        /// <summary>
        /// Gets this user's identity provider. Note that a single userHandle could be registered against
        /// multiple identity providers. In that case, this field is set to the identityProvider used
        /// for this specific incoming request
        /// </summary>
        public IdentityProviderType IdentityProvider { get; private set; }

        /// <summary>
        /// Gets this user's identity provider account id.
        /// </summary>
        public string IdentityProviderAccountId { get; private set; }

        /// <summary>
        /// Overrides the equality operator for user principals
        /// </summary>
        /// <param name="a">First user principal</param>
        /// <param name="b">Second user principal</param>
        /// <returns>true if user handle, identity provider, and identity provider account id are the same</returns>
        public static bool operator ==(UserPrincipal a, UserPrincipal b)
        {
            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.UserHandle == b.UserHandle && a.IdentityProvider == b.IdentityProvider && a.IdentityProviderAccountId == b.IdentityProviderAccountId;
        }

        /// <summary>
        /// Overrides the inequality operator for user principals
        /// </summary>
        /// <param name="a">First user principal</param>
        /// <param name="b">Second user principal</param>
        /// <returns>false user handle, identity provider, and identity provider account id are are the same</returns>
        public static bool operator !=(UserPrincipal a, UserPrincipal b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Overrides the Equls method for user principals
        /// </summary>
        /// <param name="obj">user principal</param>
        /// <returns>true if user handle, identity provider, and identity provider account id are the same</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            UserPrincipal userPrincipal = obj as UserPrincipal;
            if ((object)userPrincipal == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this.UserHandle == userPrincipal.UserHandle && this.IdentityProvider == userPrincipal.IdentityProvider && this.IdentityProviderAccountId == userPrincipal.IdentityProviderAccountId;
        }

        /// <summary>
        /// Gets the hash code of a user principal principal by xor-ing the user handle and the account id
        /// <remarks>
        /// Note it is possible for two different user principals to end up with the same has code.
        /// This would happen only when the two user principals would have the same user handle and account id, but different identity providers.
        /// We see this is as a very unlikely occurence.
        /// </remarks>
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return this.UserHandle.GetHashCode() ^ this.IdentityProviderAccountId.GetHashCode();
        }

        /// <summary>
        /// Implements conversion from IUserPrincipal to ToString
        /// </summary>
        /// <returns>the userHandle</returns>
        public override string ToString()
        {
            return this.UserHandle + '\n' + this.IdentityProvider + '\n' + this.IdentityProviderAccountId;
        }

        /// <summary>
        /// Is in role -- must be implemented because it is required by the IPrincipal interface,
        /// but currently unused.
        /// </summary>
        /// <param name="role">Role of the user</param>
        /// <returns>Always return true</returns>
        public bool IsInRole(string role)
        {
            return true;
        }

        /// <summary>
        /// Initializes the state of a user principal
        /// </summary>
        /// <param name="userHandle">user handle</param>
        /// <param name="identityProvider">the type of the identity provider</param>
        /// <param name="identityProviderAccountId">this user's account id for the identity provider</param>
        private void Setup(string userHandle, IdentityProviderType identityProvider, string identityProviderAccountId)
        {
            this.UserHandle = userHandle;
            this.IdentityProvider = identityProvider;
            this.IdentityProviderAccountId = identityProviderAccountId;
        }
    }
}
