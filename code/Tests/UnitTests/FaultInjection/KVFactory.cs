// <copyright file="KVFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    using SocialPlus.Logging;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Class to generate IKV instances for unit tests
    /// </summary>
    public class KVFactory
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
        /// Factory of Azure KeyVault client libs
        /// </summary>
        private readonly AzureKeyVaultClientFactory azureKeyVaultClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="KVFactory"/> class.
        /// </summary>
        public KVFactory()
        {
            this.azureKeyVaultClientFactory = new AzureKeyVaultClientFactory();
        }

        /// <summary>
        /// Generate a list of IKV instances for unit tests
        /// </summary>
        /// <returns>List of IKV instances</returns>
        public IEnumerable<IKV> CreateKV()
        {
            var fileSettingsReader = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName);
            var clientId = fileSettingsReader.ReadValue(EmbeddedSocialClientIdSetting);
            var vaultUrl = fileSettingsReader.ReadValue(SocialPlusVaultUrlSetting);
            var certThumbprint = fileSettingsReader.ReadValue(SocialPlusCertThumbprint);
            var storeLoc = StoreLocation.CurrentUser;
            var log = new Log(LogDestination.Debug, Log.DefaultCategoryName);

            foreach (IKeyVaultClient ikvc in this.azureKeyVaultClientFactory.CreateKVClient())
            {
                yield return new KV(log, clientId, vaultUrl, certThumbprint, storeLoc, ikvc);
            }
        }
    }
}
