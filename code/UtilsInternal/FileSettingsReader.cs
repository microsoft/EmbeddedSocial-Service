// <copyright file="FileSettingsReader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Settings reader that reads settings from app settings.
    /// All calls are synchronous.
    /// </summary>
    public class FileSettingsReader : ISettingsReader
    {
        /// <summary>
        /// dictionary of setting read from the settings file
        /// </summary>
        private Dictionary<string, string> settings;

        /// <summary>
        ///  Initializes a new instance of the <see cref="FileSettingsReader"/> class.
        /// </summary>
        /// <param name="filename">name of the settings file</param>
        public FileSettingsReader(string filename)
        {
            if (File.Exists(filename))
            {
                this.settings = CustomAppSettingsFile.LoadSettings(filename);
            }
            else
            {
                throw new FileNotFoundException(string.Format("Cannot open settings file {0}", filename));
            }
        }

        /// <summary>
        /// Read value for a setting from a local settings file.
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        public string ReadValue(string settingName)
        {
            return this.settings[settingName];
        }
    }
}
