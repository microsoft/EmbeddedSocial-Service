// <copyright file="AzureKeyVaultClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.KVLibrary
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.KeyVault;
    using SocialPlus.Utils;

    /// <summary>
    /// Simple wrapper for Azure's KeyVaultClient that uses the new IKeyVaultClient interface
    /// </summary>
    public class AzureKeyVaultClient : IKeyVaultClient
    {
        /// <summary>
        /// Internal handlers to the key vault (a key-vault client)
        /// </summary>
        private readonly KeyVaultClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureKeyVaultClient"/> class.
        /// Unlike the KeyVaultClient, it takes the full <see cref="ICertificateHelper"/> and converts it to the callback.
        /// </summary>
        /// <param name="cert">Certificate helper</param>
        public AzureKeyVaultClient(ICertificateHelper cert)
        {
            this.client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(cert.GetAccessToken));
        }

        /// <summary>
        /// Retrieves the public portion of a key plus its attributes
        /// </summary>
        /// <param name="keyIdentifier">The key identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A KeyBundle of the key and its attributes</returns>
        public virtual Task<KeyBundle> GetKeyAsync(string keyIdentifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.client.GetKeyAsync(keyIdentifier, cancellationToken);
        }

        /// <summary>
        /// Gets a secret.
        /// </summary>
        /// <param name="secretIdentifier">The URL for the secret.</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A response message containing the secret</returns>
        public virtual Task<Secret> GetSecretAsync(string secretIdentifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.client.GetSecretAsync(secretIdentifier, cancellationToken);
        }

        /// <summary>
        /// Creates a signature from a digest using the specified key in the vault
        /// </summary>
        /// <param name="vault">The URL of the vault</param>
        /// <param name="keyName">The name of the key</param>
        /// <param name="keyVersion">The version of the key (optional)</param>
        /// <param name="algorithm">The signing algorithm. For more information on possible algorithm types, see JsonWebKeySignatureAlgorithm.</param>
        /// <param name="digest">The digest value to sign</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>The signature value</returns>
        public virtual Task<KeyOperationResult> SignAsync(string vault, string keyName, string keyVersion, string algorithm, byte[] digest, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.client.SignAsync(vault, keyName, keyVersion, algorithm, digest, cancellationToken);
        }
    }
}
