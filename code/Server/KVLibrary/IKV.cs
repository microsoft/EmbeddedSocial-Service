// <copyright file="IKV.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.KVLibrary
{
    using System.Threading.Tasks;

    /// <summary>
    /// This interface describes the set of public methods to the KV library.
    /// </summary>
    public interface IKV
    {
        /// <summary>
        /// Signs a piece of data using the vaulted SocialPlus Signing Key
        /// </summary>
        /// <param name="signKeyUrl">Name of the key (URL)</param>
        /// <param name="dataToSign">Data covered by the signature</param>
        /// <returns>256-byte RSA signature</returns>
        Task<byte[]> SignAsync(string signKeyUrl, byte[] dataToSign);

        /// <summary>
        /// Verifies the signature of a piece of data. This verification uses the public part of the vaulted key.
        /// The public part was extracted in the constructor to eliminate the need to contact the vault. This improves
        /// performance and cost.
        /// </summary>
        /// <param name="signKeyUrl">Name of the key (URL)</param>
        /// <param name="dataToVerify">Data covered by the signature</param>
        /// <param name="rsaSignature">Signature (256 bytes only)</param>
        /// <returns>true if the signature checks the data; false otherwise</returns>
        Task<bool> VerifyAsync(string signKeyUrl, byte[] dataToVerify, byte[] rsaSignature);

        /// <summary>
        /// HMACs a piece of data using the vaulted SocialPlus hashingKey
        /// </summary>
        /// <param name="hashKeyURL">Url of key in Key Vault used for hashing the data.</param>
        /// <param name="dataToHash">Data to be hashed (aka HMAC-ed)</param>
        /// <returns>256 bits (32 bytes) of hash</returns>
        Task<byte[]> HashAsync(string hashKeyURL, byte[] dataToHash);

        /// <summary>
        /// Given a Url to a Key Vault-protected secret, this method returns the value stored in the key vault at the particular Url.
        /// This value is further cached in an in-memory cache. All subsequence requests for the variable are served
        /// from the in-memory cache. This method does not catch any ArgumentNullException (or any other exceptions)
        /// thrown by the Azure Key Vault service.
        /// </summary>
        /// <param name="secretUrl">Key Vault Url endpoint.</param>
        /// <returns>value stored in the key vault at the corresponding Url</returns>
        Task<string> GetSecretByUrlAsync(string secretUrl);
    }
}
