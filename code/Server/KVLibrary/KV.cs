// <copyright file="KV.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.KVLibrary
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Azure.KeyVault;
    using SocialPlus.Logging;

    /// <summary>
    /// This is KV library specific to the needs of SocialPlus. It manages a pre-created $KeyVault$
    /// which is already pre-provisioned with cryptographic keys and secrets specific to the needs of SocialPlus.
    /// The $KeyVault$ also stores all the secret configuration variables of the Social Plus service.
    /// <para>
    /// Cryptographic keys:
    /// SocialPlus has two keys: a signing key and a hashing key. This library offers a public API
    /// to sign/verify a string, or to hash a string. Signing a string is the only API call that uses
    /// the key vault on every single call. Signing a string requires the private part of the signing key
    /// which never leaves the key vault by design.
    /// On the other hand, verify and hash do not use the key vault on every operation. Instead, the
    /// library reads the public part of the signing key from the $keyvault$ and the secret-key required for
    /// hashing (the HMAC key) at $init$ time. The public-part of the signing key is used to initialize an internal
    /// <c>RSA</c> crypto provider and a hasher. Verify takes the input string, computes a digest using this hasher
    /// and then uses the <c>RSA</c> crypto provider to verify the signature.
    /// </para>
    /// <para>
    /// The secret-key required for hashing (the HMAC key) is used to initialized an internal HMAC hasher.
    /// Hashing takes the input string and uses the internal HMAC hasher to compute the hash.
    /// </para>
    /// <para>
    /// Note that the internal HMAC hasher and the internal hasher used by Verify have different roles.
    /// The former is used to hash input strings passed in by callers when calling Hash. The later is used
    /// to compute a digest on the input strings passed in by callers when calling Verify.
    /// </para>
    /// <para>
    /// Configuration variables:
    /// SocialPlus stores the values of its secret configuration variables inside Azure Key Vault service.
    /// The cloud configuration files in Social Plus all contain URLs to the actual corresponding Key Vault locations.
    /// Upon the first lookup in the key vault, this library caches the values in memory. This is done so that future
    /// lookups do not incur the cost of accessing Azure's key vault.
    /// </para>
    /// </summary>
    public class KV : IKV
    {
        /// <summary>
        /// The Client ID is used by the application to uniquely identify itself to Azure AD.
        /// </summary>
        private readonly string clientId;

        /// <summary>
        /// KV certificate thumbprint
        /// </summary>
        private readonly string certThumbprint;

        /// <summary>
        /// Location of certificate store where certificate is stored
        /// </summary>
        private readonly StoreLocation storeLocation;

        /// <summary>
        /// The vaultURL is listed when creating the new azure Key Vault. You can also obtain it by issuing
        /// "Get-AzureKeyVault -VaultName 'SocialPlusKeyVault'" in Azure PS.
        /// </summary>
        private readonly string vaultUrl;

        /// <summary>
        /// Internal handlers to the key vault (a key-vault client)
        /// </summary>
        private readonly IKeyVaultClient client;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Dictionary of secrets found in the key vault. Key is the Id (a URL) and value is the secret's value
        /// </summary>
        private ConcurrentDictionary<string, string> dictionarySecretsInKV;

        /// <summary>
        /// Dictionary of keys found in the key vault. Key is the Id (a URL) and value is a tuple of the key and the <c>RSA</c> parameters.
        /// </summary>
        private ConcurrentDictionary<string, Tuple<KeyBundle, RSAParameters>> dictionaryKeysInKV;

        /// <summary>
        /// Initializes a new instance of the <see cref="KV"/> class.
        /// External method that initializes the KV library with the appropriate credentials to access the KV
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="clientID">ID of the client</param>
        /// <param name="vaultUrl">URL of the vault</param>
        /// <param name="certThumbprint">thumbprint of certificate used to access KV</param>
        /// <param name="storeLocation">store where certificate will be found: typically current user or store location store</param>
        /// <param name="client">Key Vault Client (implements IKeyVaultClient)</param>
        public KV(ILog log, string clientID, string vaultUrl, string certThumbprint, StoreLocation storeLocation, IKeyVaultClient client)
        {
            this.log = log;

            if (clientID == null || vaultUrl == null || certThumbprint == null || client == null)
            {
                throw new ArgumentNullException(string.Format(CultureInfo.InvariantCulture, "Null argument detected. clientID {0}, vaultUrl {1}, certThumbprint {2} and client {3} cannot be null", clientID, vaultUrl, certThumbprint, client));
            }

            this.clientId = clientID;
            this.vaultUrl = vaultUrl;
            this.certThumbprint = certThumbprint;
            this.storeLocation = storeLocation;
            this.client = client;

            // New the dictionaries data structures
            this.dictionaryKeysInKV = new ConcurrentDictionary<string, Tuple<KeyBundle, RSAParameters>>();
            this.dictionarySecretsInKV = new ConcurrentDictionary<string, string>();
        }

        /// <summary>
        /// Signs a piece of data using the vaulted SocialPlus Signing Key
        /// </summary>
        /// <param name="signKeyUrl">Name of the key (URL)</param>
        /// <param name="dataToSign">Data covered by the signature</param>
        /// <returns>256-byte RSA signature</returns>
        public async Task<byte[]> SignAsync(string signKeyUrl, byte[] dataToSign)
        {
            if (signKeyUrl == null || dataToSign == null)
            {
                throw new ArgumentNullException();
            }

            if (this.client == null)
            {
                throw new InvalidOperationException("client is null in KV.Sign");
            }

            // Hash data and call async. By creating our own hasher, we ensure that hashing remains thread-safe
            byte[] digest;
            KeyOperationResult signature;
            using (SHA256CryptoServiceProvider hasherForSigning = new SHA256CryptoServiceProvider())
            {
                digest = hasherForSigning.ComputeHash(dataToSign);
            }

            if (!this.dictionaryKeysInKV.ContainsKey(signKeyUrl))
            {
                KeyBundle kb = null;
                try
                {
                    kb = await this.client.GetKeyAsync(signKeyUrl);
                }
                catch (Exception e)
                {
                    this.log.LogException(string.Format("signKeyUrl {0} does not point to a valid key", signKeyUrl), e);
                }

                // Construct the RSA parameters
                var rsaParams = new RSAParameters()
                {
                    Modulus = kb.Key.N,
                    Exponent = kb.Key.E
                };

                // Construct a tuple that includes both the key bundle and the rsa parameters
                var keyBundleRsaParamsTuple = new Tuple<KeyBundle, RSAParameters>(kb, rsaParams);

                // Add tuple to dictionary
                this.dictionaryKeysInKV.TryAdd(signKeyUrl, keyBundleRsaParamsTuple);
            }

            Tuple<KeyBundle, RSAParameters> signingKey = this.dictionaryKeysInKV[signKeyUrl];

            // key vault client is thread-safe (according to comments in the Azure SDK codebase)
            signature = await this.client.SignAsync(this.vaultUrl, signingKey.Item1.KeyIdentifier.Name, signingKey.Item1.KeyIdentifier.Version, "RS256", digest);
            return signature.Result;
        }

        /// <summary>
        /// Verifies the signature of a piece of data. This verification uses the public part of the vaulted key.
        /// The public part was extracted in the constructor to eliminate the need to contact the vault. This improves
        /// performance and cost.
        /// </summary>
        /// <param name="signKeyUrl">Name of the key (URL)</param>
        /// <param name="dataToVerify">Data covered by the signature</param>
        /// <param name="rsaSignature">Signature (256 bytes only)</param>
        /// <returns>true if the signature checks the data; false otherwise</returns>
        public async Task<bool> VerifyAsync(string signKeyUrl, byte[] dataToVerify, byte[] rsaSignature)
        {
            if (signKeyUrl == null || dataToVerify == null || rsaSignature == null)
            {
                throw new ArgumentNullException();
            }

            // Hash data. By creating our own hasher, we ensure that hashing remains thread-safe
            byte[] digest;
            using (SHA256CryptoServiceProvider hasherForSigning = new SHA256CryptoServiceProvider())
            {
                digest = hasherForSigning.ComputeHash(dataToVerify);
            }

            Tuple<KeyBundle, RSAParameters> signingKey = null;
            try
            {
                signingKey = await this.GetKeyByUrlAsync(signKeyUrl);
            }
            catch (Exception e)
            {
                this.log.LogException(string.Format("signKeyUrl {0} does not point to a valid key", signKeyUrl), e);
            }

            // Verify hash. By creating our own RSA provider, we ensure that verifying the hash remains thread-safe
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(signingKey.Item2);
                return rsa.VerifyHash(digest, "sha256", rsaSignature);
            }
        }

        /// <summary>
        /// HMACs a piece of data using the vaulted SocialPlus hashingKey
        /// </summary>
        /// <param name="hashKeyURL">Url of key in Key Vault used for hashing the data.</param>
        /// <param name="dataToHash">Data to be hashed (aka HMAC-ed)</param>
        /// <returns>256 bits (32 bytes) of hash</returns>
        public async Task<byte[]> HashAsync(string hashKeyURL, byte[] dataToHash)
        {
            if (hashKeyURL == null || dataToHash == null)
            {
                throw new ArgumentNullException();
            }

            // This can throw KeyNotFound. No need to catch it here.
            // The HashingKey in the key-vault is a 32-bit password (keyboard characters). It is not base-64 encoded.
            byte[] hmacKey = null;
            try
            {
                hmacKey = Encoding.ASCII.GetBytes(await this.GetSecretByUrlAsync(hashKeyURL));
            }
            catch (Exception e)
            {
                this.log.LogException(string.Format("hashKeyUrl {0} does not point to a valid key", hashKeyURL), e);
            }

            if (hmacKey.Length != 32)
            {
                throw new ConfigurationErrorsException(string.Format("The hashing key's length is not 32 bytes. Instead it is {0}", hmacKey.Length));
            }

            // Hash data. By creating our own hasher, we ensure that hashing remains thread-safe
            using (HMACSHA256 hasherHMAC = new HMACSHA256(hmacKey))
            {
                return hasherHMAC.ComputeHash(dataToHash);
            }
        }

        /// <summary>
        /// Given a Url to a Key Vault-protected secret, this method returns the value stored in the key vault at the particular Url.
        /// This value is further cached in an in-memory cache. All subsequence requests for the variable are served
        /// from the in-memory cache. This method does not catch any ArgumentNullException (or any other exceptions)
        /// thrown by the Azure Key Vault service.
        /// </summary>
        /// <param name="secretUrl">Key Vault Url endpoint.</param>
        /// <returns>value stored in the key vault at the corresponding Url</returns>
        public async Task<string> GetSecretByUrlAsync(string secretUrl)
        {
            if (secretUrl == null)
            {
                throw new ArgumentNullException();
            }

            if (!this.dictionarySecretsInKV.ContainsKey(secretUrl))
            {
                if (this.client == null)
                {
                    this.log.LogException(new InvalidOperationException("client is null in KV.GetSecretByUrl"));
                }

                // Fetch the value from the Key Vault. Note that GetSecretAsync could throw ArgumentNullException if key vault cannot be accessed
                try
                {
                    Secret secret = await this.client.GetSecretAsync(secretUrl);
                    this.dictionarySecretsInKV.TryAdd(secretUrl, secret.Value);
                }
                catch (Exception ex)
                {
                    string errorMsg = string.Format("Can't retrieve key {0} from Azure KV {1}.", secretUrl, this.vaultUrl);
                    errorMsg += string.Format("\nCert thumbprint {0}, client ID {1}.", this.certThumbprint, this.clientId);
                    this.log.LogException(errorMsg, ex);
                }
            }

            return this.dictionarySecretsInKV[secretUrl];
        }

        /// <summary>
        /// Given a Url to a Key Vault-protected key, this method returns the material stored in the key vault at the particular URL.
        /// This value is further cached in an in-memory cache. All subsequence requests for the variable are served
        /// from the in-memory cache. This method does not catch any ArgumentNullException (or any other exceptions)
        /// thrown by the Azure Key Vault service.
        /// </summary>
        /// <param name="keyUrl">Key Vault Url endpoint.</param>
        /// <returns>a tuple representing the <c>keybundle</c> and the RSA parameters associated with the key</returns>
        private async Task<Tuple<KeyBundle, RSAParameters>> GetKeyByUrlAsync(string keyUrl)
        {
            if (!this.dictionaryKeysInKV.ContainsKey(keyUrl))
            {
                if (this.client == null)
                {
                    this.log.LogException(new InvalidOperationException("client is null in KV.GetSecretByUrl"));
                }

                KeyBundle keyBundle = await this.client.GetKeyAsync(keyUrl);
                RSAParameters rsaParameters = new RSAParameters() { Modulus = keyBundle.Key.N, Exponent = keyBundle.Key.E };
                Tuple<KeyBundle, RSAParameters> key = new Tuple<KeyBundle, RSAParameters>(keyBundle, rsaParameters);

                this.dictionaryKeysInKV.TryAdd(keyUrl, key);
            }

            return this.dictionaryKeysInKV[keyUrl];
        }
    }
}
