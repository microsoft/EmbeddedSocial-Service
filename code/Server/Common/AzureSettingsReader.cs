// <copyright file="AzureSettingsReader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server
{
    using Microsoft.Azure;
    using SocialPlus.Utils;

    /// <summary>
    /// Settings reader that reads settings from cloud configuration manager
    /// This reader does NOT use Azure key vault to retrieve secure settings.
    /// All calls are synchronous.
    /// </summary>
    public class AzureSettingsReader : ISettingsReader
    {
        /// <summary>
        /// Flag determining whether CloudConfigurationManager tracing should be enabled
        /// </summary>
        private bool isTracingEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSettingsReader"/> class.
        /// </summary>
        /// <param name="isTracingEnabled">Flag determining whether to enable tracing</param>
        public AzureSettingsReader(bool isTracingEnabled)
        {
            this.isTracingEnabled = isTracingEnabled;
        }

        /// <summary>
        /// Read value for a setting from the CloudConfigurationManager
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        public string ReadValue(string settingName)
        {
            // GetSetting could throw an exception if settingName is null or empty.
            // We do not catch or handle the exception here.
            return CloudConfigurationManager.GetSetting(settingName, this.isTracingEnabled);
        }
    }
}
