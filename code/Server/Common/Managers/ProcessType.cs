// <copyright file="ProcessType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Managers
{
    /// <summary>
    /// Process type for a call
    /// </summary>
    public enum ProcessType
    {
        /// <summary>
        /// The processing is in the critical path of the user
        /// </summary>
        Frontend,

        /// <summary>
        /// The processing is in the background and is the first try
        /// </summary>
        Backend,

        /// <summary>
        /// The processing is in the background and is a retry
        /// </summary>
        BackendRetry
    }
}
