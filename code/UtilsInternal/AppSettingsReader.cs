// <copyright file="AppSettingsReader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Configuration;

    /// <summary>
    /// Settings reader that reads settings from app settings.
    /// All calls are synchronous.
    /// </summary>
    public class AppSettingsReader : ISettingsReader
    {
        /// <summary>
        /// Read value for a setting from ConfigurationManager.AppSettings.
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        public string ReadValue(string settingName)
        {
            return ConfigurationManager.AppSettings[settingName];
        }
    }
}
