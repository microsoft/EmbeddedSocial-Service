// <copyright file="KVSettingsReader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.KVLibrary;

    /// <summary>
    /// Settings reader that reads settings from crypto vaults such as Azure's Key Vault service.
    /// Its methods are async
    /// </summary>
    public class KVSettingsReader : ISettingsReaderAsync
    {
        /// <summary>
        /// All settings that are secured by the KV must have their values start with a specially designed prefix.
        /// </summary>
        private readonly string secureValuePrefix = "kv:";

        /// <summary>
        /// key vault
        /// </summary>
        private readonly IKV kv;

        /// <summary>
        /// synchronous settings reader
        /// </summary>
        private readonly ISettingsReader syncSettingsReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="KVSettingsReader"/> class.
        /// </summary>
        /// <param name="syncSettingsReader">synchronous settings reader</param>
        /// <param name="kv">key vault</param>
        public KVSettingsReader(ISettingsReader syncSettingsReader, IKV kv)
        {
            this.syncSettingsReader = syncSettingsReader;
            this.kv = kv;
        }

        /// <summary>
        /// Read value for a setting. This method reads from the synchronous reader first.
        /// If the value starts with the secureValuePrefix, it then calls KV to retrieve the
        /// corresponding value from Azure's Key Vault.
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        public async Task<string> ReadValueAsync(string settingName)
        {
            string settingValue = null;

            // Call the synchronous reader first
            settingValue = this.syncSettingsReader.ReadValue(settingName);

            // If settingValue is null, the current state of our configuration files is wromg (or we have a bug)
            if (settingValue == null)
            {
                throw new InvalidOperationException(string.Format("Setting: {0} not defined in the configuration files.", settingName));
            }

            // If the value starts with a secureValuePrefix, this means that the value is simply a KV address.
            // In that case, call into the KV library to
            if (settingValue.StartsWith(this.secureValuePrefix))
            {
                settingValue = await this.kv.GetSecretByUrlAsync(settingValue.Substring(this.secureValuePrefix.Length));
            }

            return settingValue;
        }
    }
}
