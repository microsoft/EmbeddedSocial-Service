// <copyright file="CertificateHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Util class for logic implementing cert-based authentication using AAD. Most of the logic is based on:
    /// http://blogs.msdn.com/b/microsoft_azure_simplified/archive/2015/03/23/getting-started-using-azure-active-directory-aad-for-authenticating-automated-clients-c.aspx
    /// </summary>
    public class CertificateHelper : ICertificateHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateHelper"/> class.
        /// Looks up the certificate in the constructor and saves it in an internal variable
        /// </summary>
        /// <param name="certThumbprint">certificate's thumbprint</param>
        /// <param name="clientID">client ID of the caller who will request tokens</param>
        /// <param name="storeLocation">store where certificate will be found: typically current user or store location store</param>
        public CertificateHelper(string certThumbprint, string clientID, StoreLocation storeLocation)
        {
            if (string.IsNullOrEmpty(certThumbprint))
            {
                throw new Exception("Thumbprint field is missing in the settings. Please fix and try again.");
            }

            X509Certificate2 clientAssertionCertPfx = this.FindCertificateByThumbprint(certThumbprint, storeLocation);
            if (clientAssertionCertPfx == null)
            {
                throw new Exception(string.Format("Certificate with thumbprint {0} is not installed.", certThumbprint));
            }

            this.Cert = new ClientAssertionCertificate(clientID, clientAssertionCertPfx);
        }

        /// <summary>
        /// Gets or sets certificate
        /// </summary>
        private ClientAssertionCertificate Cert { get; set; }

        /// <summary>
        /// Gets AAD access token. This call takes a third parameter that remains unused.
        /// The KeyVaultClient Azure SDK library requires the GetAccessToken to have a third parameter
        /// even though it does not seem to be used anywhere.
        /// </summary>
        /// <param name="authority">authority</param>
        /// <param name="resource">resource</param>
        /// <param name="scope">The scop of the authentication request</param>
        /// <returns>access token</returns>
        public async Task<string> GetAccessToken(string authority, string resource, string scope = null)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, this.Cert);
            return result.AccessToken;
        }

        /// <summary>
        /// Finds certificate in local certificate store by its thumbprint.
        /// Note that the StoreLocation is CurrentUser instead of LocalMachine.
        /// And that we are supplying 'false' to the Find method because we are using a test cert.
        /// </summary>
        /// <param name="thumbprint">certificate's thumbprint</param>
        /// <param name="storeLocation">store where certificate will be found: typically current user or store location store</param>
        /// <returns>an X509 certificate</returns>
        private X509Certificate2 FindCertificateByThumbprint(string thumbprint, StoreLocation storeLocation)
        {
            X509Store store = new X509Store(StoreName.My, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false); // Don't validate certs, since the test root isn't installed.
                if (col == null || col.Count == 0)
                {
                    return null;
                }

                return col[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
