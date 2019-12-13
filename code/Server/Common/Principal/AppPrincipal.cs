// <copyright file="AppPrincipal.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Principal
{
    using System;
    using System.Security.Principal;

    /// <summary>
    /// Default implementation of IAppPrincipal. An application principal is the authenticated entity that corresponds
    /// to an application. It consists of a "valid" app key and an app handle. The authentication filter did validate the
    /// app key and app handle of the incoming request. Valid is in quotes because we cannot guarantee that the app key was not
    /// deleted in between the authentication filter's check and the controller executing.
    /// </summary>
    public class AppPrincipal : IPrincipal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppPrincipal"/> class
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        public AppPrincipal(string appHandle, string appKey)
        {
            this.Setup(appHandle, appKey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppPrincipal"/> class
        /// </summary>
        /// <param name="serializedAppPrincipal">the app principal in a serialized form</param>
        public AppPrincipal(string serializedAppPrincipal)
        {
            if (!serializedAppPrincipal.Contains("\n"))
            {
                throw new ArgumentException("The convertedAppPrincipal parameter has no newline character in it.");
            }

            // Call internal util method split on delimeter. This is guaranteed to return at least two substrings
            string[] splitOnNewline = serializedAppPrincipal.Split('\n');
            this.Setup(splitOnNewline[0], splitOnNewline[1]);
        }

        /// <summary>
        /// Gets identity for principal. Must be implemented because it is required by the IPrincipal interface,
        /// but it is unused by our service
        /// </summary>
        public IIdentity Identity { get; private set; }

        /// <summary>
        /// Gets app handle
        /// </summary>
        public string AppHandle { get; private set; }

        /// <summary>
        /// Gets app key
        /// </summary>
        public string AppKey { get; private set; }

        /// <summary>
        /// Overrides the equality operator for app principals
        /// </summary>
        /// <param name="a">First app principal</param>
        /// <param name="b">Second app principal</param>
        /// <returns>true if both app handle and app key are the same</returns>
        public static bool operator ==(AppPrincipal a, AppPrincipal b)
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
            return a.AppHandle == b.AppHandle && a.AppKey == b.AppKey;
        }

        /// <summary>
        /// Overrides the inequality operator for app principals
        /// </summary>
        /// <param name="a">First app principal</param>
        /// <param name="b">Second app principal</param>
        /// <returns>false if both app handle and app key are the same</returns>
        public static bool operator !=(AppPrincipal a, AppPrincipal b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Overrides the Equls method for app principals
        /// </summary>
        /// <param name="obj">App principal</param>
        /// <returns>true if both app handle and app key are the same</returns>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            AppPrincipal appPrincipal = obj as AppPrincipal;
            if ((object)appPrincipal == null)
            {
                return false;
            }

            // Return true if the fields match:
            return this.AppHandle == appPrincipal.AppHandle && this.AppKey == appPrincipal.AppKey;
        }

        /// <summary>
        /// Gets the hash code of an app principal by xor-ing app handles and app key
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return this.AppHandle.GetHashCode() ^ this.AppKey.GetHashCode();
        }

        /// <summary>
        /// Implements conversion from IAppPrincipal to ToString
        /// </summary>
        /// <returns>the appHandle followed by a newline followed by appKey</returns>
        public override string ToString()
        {
            return this.AppHandle + '\n' + this.AppKey;
        }

        /// <summary>
        /// Is in role -- must be implemented because it is required by the IPrincipal interface,
        /// but currently unused.
        /// </summary>
        /// <param name="role">Role of the app</param>
        /// <returns>Always return true</returns>
        public bool IsInRole(string role)
        {
            return true;
        }

        /// <summary>
        /// Initializes internal state of this class
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="appKey">App key</param>
        private void Setup(string appHandle, string appKey)
        {
            this.AppHandle = appHandle;
            this.AppKey = appKey;
        }
    }
}
