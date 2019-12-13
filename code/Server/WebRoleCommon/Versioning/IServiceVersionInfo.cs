// <copyright file="IServiceVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System.Collections.Generic;

    /// <summary>
    /// Information about all valid API versions supported by a service
    /// </summary>
    public interface IServiceVersionInfo
    {
        /// <summary>
        /// Gets lowest major version number.  All major and minor version numbers must be non-negative.
        /// </summary>
        int FirstMajorVersion { get; }

        /// <summary>
        /// Gets number of major versions.
        /// </summary>
        int NumMajorVersions { get; }

        /// <summary>
        /// Gets array that records the valid minor version numbers for a given major version number.
        /// This array is always indexed by the major version number.
        /// </summary>
        MinorVersionInfo[] ValidVersions { get; }

        /// <summary>
        /// Returns the current service version number.
        /// Current is the always the maximum valid version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the current service version number</returns>
        ApiVersionInfo GetCurrentVersion();

        /// <summary>
        /// Returns the maximum service version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the maximum service version number</returns>
        ApiVersionInfo GetMaxVersion();

        /// <summary>
        /// Returns the minimum valid service version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the minimum service version number</returns>
        ApiVersionInfo GetMinVersion();

        /// <summary>
        /// Checks whether a version is valid.
        /// </summary>
        /// <param name="vers">The version to validate</param>
        /// <returns>True if <c>vers</c> is valid, false otherwise</returns>
        bool Validate(ApiVersionInfo vers);

        /// <summary>
        /// Enumerates all the valid versions in the range from start to end
        /// </summary>
        /// <param name="start">the starting version of the range</param>
        /// <param name="end">the ending version of the range</param>
        /// <returns>a list of all valid versions in the range from start to end</returns>
        List<string> EnumerateVersions(ApiVersionInfo start, ApiVersionInfo end);
    }
}
