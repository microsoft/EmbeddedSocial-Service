// <copyright file="ICertificateHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for utilities for implementing cert-based authentication using AAD. Most of the logic is based on:
    /// http://blogs.msdn.com/b/microsoft_azure_simplified/archive/2015/03/23/getting-started-using-azure-active-directory-aad-for-authenticating-automated-clients-c.aspx
    /// </summary>
    public interface ICertificateHelper
    {
        /// <summary>
        /// Gets AAD access token.
        /// This call takes an optional third parameter that remains unused because
        /// the KeyVaultClient Azure SDK library requires the GetAccessToken to have a third parameter
        /// even though it does not seem to be used anywhere.
        /// </summary>
        /// <param name="authority">authority</param>
        /// <param name="resource">resource</param>
        /// <param name="scope">The scop of the authentication request</param>
        /// <returns>access token</returns>
        Task<string> GetAccessToken(string authority, string resource, string scope = null);
    }
}
