// <copyright file="BuildsCurrentResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Response from get builds current
    /// </summary>
    public class BuildsCurrentResponse
    {
        /// <summary>
        /// Gets or sets the date and time of the current build
        /// </summary>
        public string DateAndTime { get; set; }

        /// <summary>
        /// Gets or sets the Git commit hash that represents the current git head
        /// </summary>
        public string CommitHash { get; set; }

        /// <summary>
        /// Gets or sets the hostname that this code was built on
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets service api version number
        /// </summary>
        public string ServiceApiVersion { get; set; }

        /// <summary>
        /// Gets or sets the list of files that were not committed at build time
        /// </summary>
        public List<string> DirtyFiles { get; set; }
    }
}
