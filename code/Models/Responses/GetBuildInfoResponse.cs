// <copyright file="GetBuildInfoResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Response from get build info
    /// </summary>
    public class GetBuildInfoResponse
    {
        /// <summary>
        /// Gets or sets the date and time of the current build
        /// </summary>
        public string DateAndTime { get; set; }

        /// <summary>
        /// Gets or sets the Git commit hash that represents the git head
        /// </summary>
        public string CommitHash { get; set; }

        /// <summary>
        /// Gets or sets the hostname that this code was built on
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the name of the branch the code was built from
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with the commit hash (if any)
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the list of files that were not committed at build time
        /// </summary>
        public List<string> DirtyFiles { get; set; }
    }
}
