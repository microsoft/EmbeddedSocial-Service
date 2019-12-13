//-----------------------------------------------------------------------
// <copyright file="VersionRangeAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements the <c>VersionRangeAttribute</c> class for API versioning.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRole.Versioning
{
    using System;

    using SocialPlus.Server.DependencyResolution;
    using SocialPlus.Server.WebRoleCommon.DependencyResolution;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using StructureMap;

    /// <summary>
    /// The VersionRangeAttribute class provides a compact way of specifying a range of versions that are supported by a class or method.
    /// Currently, we use these version ranges as attributes on controller classes to specify which controllers provide implementations for which API versions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class VersionRangeAttribute : BaseVersionRangeAttribute
    {
        // possible values for the versionrange string:
        // [VersionRange("All")]              : all versions
        // [VersionRange("v3.0-Cur")]         : version 3.0 to present
        // [VersionRange("v1.1")]             : single version
        // [VersionRange("v0.8-v2.2")]        : explicit range
        // [VersionRange("Min-v1.7")]         : begin to v1.7

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionRangeAttribute"/> class.
        /// </summary>
        /// <param name="versionRange">attribute string for the version range</param>
        public VersionRangeAttribute(string versionRange)
            : base(GetServiceVersionInfo(), versionRange)
        {
        }

        /// <summary>
        /// Gets service version info
        /// </summary>
        /// <returns>service version info</returns>
        private static IServiceVersionInfo GetServiceVersionInfo()
        {
            return IoC<Registry>.ServiceVersionInfo;
        }
    }
}