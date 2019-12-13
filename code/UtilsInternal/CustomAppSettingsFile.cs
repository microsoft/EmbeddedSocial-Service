// <copyright file="CustomAppSettingsFile.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    /// This class reads an app settings file from a custom location
    /// </summary>
    public class CustomAppSettingsFile
    {
        /// <summary>
        /// Loads the contents of the appSettings file into a dictionary
        /// </summary>
        /// <param name="filePath">path to the appSettings file</param>
        /// <returns>The dictionary</returns>
        public static Dictionary<string, string> LoadSettings(string filePath)
        {
            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = filePath;
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var appSettings = config.AppSettings.Settings;
            return ConvertToDictionary(appSettings);
        }

        /// <summary>
        /// Converts a <c>KeyValueConfigurationCollection</c> into a Dictionary.
        /// </summary>
        /// <param name="settings">The collection to convert</param>
        /// <returns>The dictionary</returns>
        private static Dictionary<string, string> ConvertToDictionary(KeyValueConfigurationCollection settings)
        {
            if (settings == null)
            {
                return null;
            }

            var newCollection = new Dictionary<string, string>();
            IEnumerable<string> allKeys = settings.AllKeys;
            foreach (string entry in allKeys)
            {
                newCollection.Add(entry, settings[entry].Value);
            }

            return newCollection;
        }
    }
}
