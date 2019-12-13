// <copyright file="ISettingsReaderAsync.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System.Threading.Tasks;

    /// <summary>
    /// Settings reader that reads settings using asynchronous calls
    /// </summary>
    public interface ISettingsReaderAsync
    {
        /// <summary>
        /// Read value for a setting asynchronously
        /// </summary>
        /// <param name="settingName">Setting name</param>
        /// <returns>Setting value</returns>
        Task<string> ReadValueAsync(string settingName);
    }
}