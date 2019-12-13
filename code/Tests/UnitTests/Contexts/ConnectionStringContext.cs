// <copyright file="ConnectionStringContext.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.UnitTests
{
    using System.Configuration;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    using SocialPlus;
    using SocialPlus.Logging;
    using SocialPlus.Server;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Utils;

    /// <summary>
    /// Class encapsulating the context needed for a connection string provider
    /// </summary>
    public class ConnectionStringContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringContext"/> class.
        /// </summary>
        public ConnectionStringContext()
        {
            this.HandleGenerator = new HandleGenerator();
            this.Log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            this.FileSettingsReader = new FileSettingsReader(ConfigurationManager.AppSettings["ConfigRelativePath"] + Path.DirectorySeparatorChar + TestConstants.ConfigFileName);

            string clientId = this.FileSettingsReader.ReadValue("AADEmbeddedSocialClientId");
            string certThumbprint = this.FileSettingsReader.ReadValue("SocialPlusCertThumbprint");
            var cert = new CertificateHelper(certThumbprint, clientId, StoreLocation.CurrentUser);
            AzureKeyVaultClient keyVaultClientLib = new AzureKeyVaultClient(cert);

            string vaultUrl = this.FileSettingsReader.ReadValue("KeyVaultUri");
            this.KeyVault = new KV(this.Log, clientId, vaultUrl, certThumbprint, StoreLocation.CurrentUser, keyVaultClientLib);

            // Initialize connection string provider
            this.KVSettingsReader = new KVSettingsReader(this.FileSettingsReader, this.KeyVault);
            this.ConnectionStringProvider = new ConnectionStringProvider(this.KVSettingsReader);
        }

        /// <summary>
        /// Gets handle generator
        /// </summary>
        public HandleGenerator HandleGenerator { get; private set; }

        /// <summary>
        /// Gets Log
        /// </summary>
        public Log Log { get; private set; }

        /// <summary>
        /// Gets KV Settings reader
        /// </summary>
        public KVSettingsReader KVSettingsReader { get; private set; }

        /// <summary>
        /// Gets reader for the settings
        /// </summary>
        public FileSettingsReader FileSettingsReader { get; private set; }

        /// <summary>
        /// Gets connection string provider
        /// </summary>
        public ConnectionStringProvider ConnectionStringProvider { get; private set; }

        /// <summary>
        /// Gets the KV library
        /// </summary>
        public KV KeyVault { get; private set; }
    }
}
