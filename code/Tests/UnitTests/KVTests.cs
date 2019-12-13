// <copyright file="KVTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;

    using Microsoft.Azure.KeyVault;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.TestUtils;
    using SocialPlus.Utils;

    /// <summary>
    /// All unit tests for KV testing
    /// </summary>
    [TestClass]
    public class KVTests
    {
        /// <summary>
        /// String used to name the setting of the Azure AD client
        /// </summary>
        private static readonly string EmbeddedSocialClientIdSetting = "AADEmbeddedSocialClientId";

        /// <summary>
        /// String used to name the setting of the Azure AD client secret
        /// </summary>
        private static readonly string SocialPlusCertThumbprint = "SocialPlusCertThumbprint";

        /// <summary>
        /// String used to name the setting of the URL to access keyvault
        /// </summary>
        private static readonly string SocialPlusVaultUrlSetting = "KeyVaultUri";

        /// <summary>
        /// String used to name the setting of the bearer token signing key
        /// </summary>
        private static readonly string BearerTokenSigningKey = "SigningKey";

        /// <summary>
        /// String used to name the hashing key id.
        /// </summary>
        private static readonly string HashingKeyId = "HashingKey";

        /// <summary>
        /// Random byte generator
        /// </summary>
        private static readonly RandUtils Rand = new RandUtils();

        /// <summary>
        /// Reader for the settings
        /// </summary>
        private static readonly FileSettingsReader FileSettingsReader;

        /// <summary>
        /// The Client ID is used by the application to uniquely identify itself to Azure AD.
        /// </summary>
        private static readonly string ClientId;

        /// <summary>
        /// KV certificate thumbprint
        /// </summary>
        private static readonly string CertThumbprint;

        /// <summary>
        /// Location of certificate store where certificate is stored
        /// </summary>
        private static readonly StoreLocation StoreLoc;

        /// <summary>
        /// The vaultURL is listed when creating the new Azure Key Vault. You can also obtain it by issuing
        /// "Get-AzureKeyVault -VaultName 'SocialPlusKeyVault'" in Azure PS.
        /// </summary>
        private static readonly string VaultUrl;

        /// <summary>
        /// Signing key
        /// </summary>
        private static readonly string SigningKey;

        /// <summary>
        /// Client for static (Test Explorer) tests only.
        /// </summary>
        private static readonly IKeyVaultClient Client;

        /// <summary>
        /// Log
        /// </summary>
        private static readonly ILog TestLog;

        /// <summary>
        /// The KV library
        /// </summary>
        private readonly IKV kv;

        /// <summary>
        /// Initializes static members of the <see cref="KVTests"/> class.
        /// </summary>
        static KVTests()
        {
            FileSettingsReader = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName);
            SigningKey = FileSettingsReader.ReadValue(BearerTokenSigningKey);
            ClientId = FileSettingsReader.ReadValue(EmbeddedSocialClientIdSetting);
            VaultUrl = FileSettingsReader.ReadValue(SocialPlusVaultUrlSetting);
            CertThumbprint = FileSettingsReader.ReadValue(SocialPlusCertThumbprint);
            StoreLoc = StoreLocation.CurrentUser;
            var cert = new CertificateHelper(CertThumbprint, ClientId, StoreLoc);
            Client = new AzureKeyVaultClient(cert);
            TestLog = new Log(LogDestination.Debug, Log.DefaultCategoryName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVTests"/> class.
        /// Uses the information from the settings file.
        /// </summary>
        public KVTests()
        {
            this.kv = new KV(TestLog, ClientId, VaultUrl, CertThumbprint, StoreLoc, Client);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KVTests"/> class.
        /// </summary>
        /// <param name="kv">An <see cref="IKV"/> instance to use for the unit tests.</param>
        public KVTests(IKV kv)
        {
            this.kv = kv;
        }

        /// <summary>
        /// Very fast test for KV Sign and Verify
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignVerifyQuickTest()
        {
            // Sign/verify 1KB
            await this.KVSignVerifyKConcurrentRandomData(1 * 1024, 1);
        }

        /// <summary>
        /// Slow KV Sign test
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignVerifySlowTest()
        {
            // Number of concurrent tasks
            int cConcCalls = 100;

            // Sign/verify 1KB
            await this.KVSignVerifyKConcurrentRandomData(1 * 1024, cConcCalls);
        }

        /// <summary>
        /// Slow tests of KV sign and verify with different data sizes
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignVerifyDifferentDataSizesTest()
        {
            // Do 10 rounds of sign/verify 1KB, 2KB, 4KB, 16KB, 32KB
            await this.KVSignVerifyKConcurrentRandomData(1 * 1024, 10);
            await this.KVSignVerifyKConcurrentRandomData(2 * 1024, 10);
            await this.KVSignVerifyKConcurrentRandomData(4 * 1024, 10);
            await this.KVSignVerifyKConcurrentRandomData(16 * 1024, 10);
            await this.KVSignVerifyKConcurrentRandomData(32 * 1024, 10);
        }

        /// <summary>
        /// Test for null clientid passed to constructor
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVNullConstructorClientID()
        {
            await Task.Run(() => AssertUtils.AssertThrowsException<ArgumentNullException>(() => { new KV(TestLog, null, VaultUrl, CertThumbprint, StoreLoc, Client); }));
        }

        /// <summary>
        /// Test for null vaulturl passed to constructor
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVNullConstructorVaultURL()
        {
            await Task.Run(() => AssertUtils.AssertThrowsException<ArgumentNullException>(() => { new KV(TestLog, ClientId, null, CertThumbprint, StoreLoc, Client); }));
        }

        /// <summary>
        /// Test for null certthumbprint passed to constructor
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVNullConstructorCertThumbprint()
        {
            await Task.Run(() => AssertUtils.AssertThrowsException<ArgumentNullException>(() => { new KV(TestLog, ClientId, VaultUrl, null, StoreLoc, Client); }));
        }

        /// <summary>
        /// Test for null client passed to constructor
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVNullConstructorClient()
        {
            await Task.Run(() => AssertUtils.AssertThrowsException<ArgumentNullException>(() => { new KV(TestLog, ClientId, VaultUrl, CertThumbprint, StoreLoc, null); }));
        }

        /// <summary>
        /// Test for null url to Sign
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignNullUrl()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.SignAsync(null, Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Test for null data to Sign
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignNullData()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.SignAsync(SigningKey, null); });
        }

        /// <summary>
        /// Test for null url to Verify
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVVerifyNullUrl()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.VerifyAsync(null, Rand.GenerateRandomBytes(1024), Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Test for null data to Verify
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVVerifyNullData()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.VerifyAsync(SigningKey, null, Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Test for null signature to Verify
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVVerifyNullSignature()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.VerifyAsync(SigningKey, Rand.GenerateRandomBytes(1024), null); });
        }

        /// <summary>
        /// Test for null url to Hash
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVHashNullUrl()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.HashAsync(null, Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Test for null data to Hash
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVHashNullData()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.HashAsync(SigningKey, null); });
        }

        /// <summary>
        /// Test for null url to GetSecretByUrl
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVGetSecretByUrlNull()
        {
            await AssertUtils.AssertThrowsExceptionAsync<ArgumentNullException>(async () => { await this.kv.GetSecretByUrlAsync(null); });
        }

        /// <summary>
        /// Call sign with the last byte of the key missing
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVSignWrongKey()
        {
            await AssertUtils.AssertThrowsExceptionAsync<KeyVaultClientException>(async () => { await this.kv.SignAsync(SigningKey.Remove(SigningKey.Length - 1), Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Call verify with the last byte of the key missing
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVVerifyWrongKey()
        {
            await AssertUtils.AssertThrowsExceptionAsync<KeyVaultClientException>(async () => { await this.kv.VerifyAsync(SigningKey.Remove(SigningKey.Length - 1), Rand.GenerateRandomBytes(1024), Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Call hash with the last byte of the key missing
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVHashWrongKey()
        {
            string hashingKey = FileSettingsReader.ReadValue(HashingKeyId);
            await AssertUtils.AssertThrowsExceptionAsync<KeyVaultClientException>(async () => { await this.kv.HashAsync(hashingKey.Remove(hashingKey.Length - 1), Rand.GenerateRandomBytes(1024)); });
        }

        /// <summary>
        /// Call verify on different set of random bytes - signature will be "wrong"
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [TestMethod]
        public async Task KVVerifyWrongSignature()
        {
            var signature = await this.kv.SignAsync(SigningKey, Rand.GenerateRandomBytes(1024));
            Assert.IsFalse(await this.kv.VerifyAsync(SigningKey, Rand.GenerateRandomBytes(1024), signature));
        }

        /// <summary>
        /// Fast test for Hash
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task KVHashQuickTest()
        {
            // Hash 1KB
            await this.KVHashKConcurrentRandomData(1 * 1024, 1);
        }

        /// <summary>
        /// Slow test for Hash
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [TestMethod]
        public async Task KVHashSlowTest()
        {
            // Number of concurrent tasks
            int cConcCalls = 100;

            // Hash 1KB
            await this.KVHashKConcurrentRandomData(1 * 1024, cConcCalls);
        }

        /// <summary>
        /// Generate random data, sign it and verify the signature in k calls running concurrently
        /// </summary>
        /// <param name="size">the size of the random data to be generated</param>
        /// <param name="k">number of concurrent calls</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task KVSignVerifyKConcurrentRandomData(ushort size, int k)
        {
            byte[][] randomByteArray = new byte[k][];
            Task<byte[]>[] taskArray = new Task<byte[]>[k];

            // Generate k random byte arrays
            for (int i = 0; i < k; i += 1)
            {
                randomByteArray[i] = Rand.GenerateRandomBytes(size);
            }

            // Start all signature calls back-to-back
            for (int i = 0; i < k; i += 1)
            {
                taskArray[i] = this.kv.SignAsync(SigningKey, randomByteArray[i]);
            }

            // Verify each signature one-by-one
            for (int i = 0; i < k; i += 1)
            {
                taskArray[i].Wait();
                Assert.IsTrue(await this.kv.VerifyAsync(SigningKey, randomByteArray[i], taskArray[i].Result));
            }
        }

        /// <summary>
        /// Generate random data and hash it in k calls running concurrently
        /// </summary>
        /// <param name="size">the size of the random data to be generated</param>
        /// <param name="k">number of concurrent calls</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task KVHashKConcurrentRandomData(ushort size, int k)
        {
            byte[][] randomByteArray = new byte[k][];
            Task<byte[]>[] taskArray = new Task<byte[]>[k];
            string hashingKeyId = FileSettingsReader.ReadValue(HashingKeyId);

            // Generate k random byte arrays
            for (int i = 0; i < k; i += 1)
            {
                randomByteArray[i] = Rand.GenerateRandomBytes(size);
            }

            // Start the hashing calls back-to-back
            for (int i = 0; i < k; i += 1)
            {
                taskArray[i] = this.kv.HashAsync(hashingKeyId, randomByteArray[i]);
            }

            // Check that hashing finishes and returns 32 bytes
            for (int i = 0; i < k; i += 1)
            {
                await taskArray[i];
                Assert.AreEqual(taskArray[i].Result.Length, 32);

                // Save the output back into the randomByteArrays
                randomByteArray[i] = taskArray[i].Result;
            }

            // No simple way in Linq exists to check that these byte arrays have unique values.
            // Implement an ugly check instead
            for (int i = 0; i < k; i += 1)
            {
                for (int j = i + 1; j < k; j += 1)
                {
                    Assert.IsFalse(randomByteArray[i].SequenceEqual(randomByteArray[j]));
                }
            }
        }
    }
}
