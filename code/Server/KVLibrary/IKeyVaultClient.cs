// <copyright file="IKeyVaultClient.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.KVLibrary
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Azure.KeyVault;

    /// <summary>
    /// Interface for the portion of Azure Key Vault Client used by KVLibrary.
    /// The full Azure KeyVaultClient has 32 method signatures (many method names are overloaded);
    /// this interface contains the 3 methods used by KVLibrary.
    /// </summary>
    public interface IKeyVaultClient
    {
        /// <summary>
        /// Retrieves the public portion of a key plus its attributes
        /// </summary>
        /// <param name="keyIdentifier">The key identifier</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A KeyBundle of the key and its attributes</returns>
        Task<KeyBundle> GetKeyAsync(string keyIdentifier, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets a secret.
        /// </summary>
        /// <param name="secretIdentifier">The URL for the secret.</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A response message containing the secret</returns>
        Task<Secret> GetSecretAsync(string secretIdentifier, CancellationToken cancellationToken = default(CancellationToken));

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
        Task<KeyOperationResult> SignAsync(string vault, string keyName, string keyVersion, string algorithm, byte[] digest, CancellationToken cancellationToken = default(CancellationToken));
    }
}
