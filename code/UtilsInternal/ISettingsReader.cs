// <copyright file="ISettingsReader.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    /// <summary>
    /// Settings reader that reads settings reads settings using synchronous calls
    /// </summary>
    public interface ISettingsReader
    {
        /// <summary>
        /// Read value for a setting
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        string ReadValue(string settingName);
    }
}
