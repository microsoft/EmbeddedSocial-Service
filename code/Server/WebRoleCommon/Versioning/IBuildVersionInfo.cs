// <copyright file="IBuildVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System.Collections.Generic;

    /// <summary>
    /// Information about the build version
    /// </summary>
    public interface IBuildVersionInfo
    {
        /// <summary>
        /// Gets commit hash
        /// </summary>
        string CommitHash { get; }

        /// <summary>
        /// Gets hostname
        /// </summary>
        string Hostname { get; }

        /// <summary>
        /// Gets dirty files
        /// </summary>
        List<string> DirtyFiles { get; }

        /// <summary>
        /// Gets build data and time
        /// </summary>
        string BuildDateAndTime { get; }

        /// <summary>
        /// Gets name of branch
        /// </summary>
        string BranchName { get; }

        /// <summary>
        /// Gets tag
        /// </summary>
        string Tag { get;  }
    }
}
