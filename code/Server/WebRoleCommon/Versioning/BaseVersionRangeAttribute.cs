//-----------------------------------------------------------------------
// <copyright file="BaseVersionRangeAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements the <c>VersionRangeAttribute</c> class for API versioning.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The BaseVersionRangeAttribute class provides a compact way of specifying a range of versions that are supported by a class or method.
    /// This class should be derived inside of a WebRole and named VersionRange
    /// Currently, we use these version ranges as attributes on controller classes to specify which controllers provide implementations for which API versions.
    /// </summary>
    public abstract class BaseVersionRangeAttribute : System.Attribute
    {
        // possible values for the versionrange string:
        // [VersionRange("All")]              : all versions
        // [VersionRange("v3.0-Cur")]         : version 3.0 to present
        // [VersionRange("v1.1")]             : single version
        // [VersionRange("v0.8-v2.2")]        : explicit range
        // [VersionRange("Min-v1.7")]         : begin to v1.7

        /// <summary>
        /// constant that represents the minimum service version
        /// </summary>
        private const string Min = "Min";

        /// <summary>
        /// constant that represents all valid service version numbers
        /// </summary>
        private const string All = "All";

        /// <summary>
        /// constant that represents the current (maximum) service version
        /// </summary>
        private const string Cur = "Cur";

        /// <summary>
        /// string representation of the version range
        /// </summary>
        private string versionRange;

        /// <summary>
        /// flag that indicates if there was an error parsing the range
        /// </summary>
        private bool parseError = false;

        /// <summary>
        /// flag that indicates if the version numbers specified in the range are incompatible with the
        /// service version information specified by <c>ServiceVersionInfo</c>
        /// </summary>
        private bool outOfRange = false;

        /// <summary>
        /// the minimum <c>ApiVersion</c> of the range
        /// </summary>
        private ApiVersionInfo minVersion;

        /// <summary>
        /// the maximum <c>ApiVersion</c> of the range
        /// </summary>
        private ApiVersionInfo maxVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseVersionRangeAttribute"/> class.
        /// </summary>
        /// <param name="serviceVersionInfo">service version info</param>
        /// <param name="versionRange">attribute string for the version range</param>
        public BaseVersionRangeAttribute(IServiceVersionInfo serviceVersionInfo, string versionRange)
        {
            this.versionRange = versionRange;

            // first, parse the version range string to make sure it has the correct format
            bool result = this.ParseVersionRange(serviceVersionInfo, this.versionRange);

            // if versionRange parses OK, then check that the range numbers are compatible with the service version info
            if (result)
            {
                this.outOfRange = !this.IsRangeValid(serviceVersionInfo);
            }

            if (this.outOfRange || this.parseError)
            {
                this.minVersion = new ApiVersionInfo(0, 0);
                this.maxVersion = new ApiVersionInfo(0, 0);
            }
        }

        /// <summary>
        /// Enumerates the list of versions specified by the version range
        /// </summary>
        /// <param name="serviceVersionInfo">info on version of the service</param>
        /// <returns>Returns a list of strings, where each string is a version in the range</returns>
        public List<string> EnumerateVersions(IServiceVersionInfo serviceVersionInfo)
        {
            if (this.parseError || this.outOfRange)
            {
                return null;
            }

            return serviceVersionInfo.EnumerateVersions(this.minVersion, this.maxVersion);
        }

        /// <summary>
        /// Checks if the version range specified by the string is compatible with the supported versions as specified by
        /// the <c>SystemVersionInfo</c> class.
        /// </summary>
        /// <param name="serviceVersionInfo">service version info</param>
        /// <returns>true if the range is valid</returns>
        private bool IsRangeValid(IServiceVersionInfo serviceVersionInfo)
        {
            // next, make sure the version range numbers are compatible with the service version info
            if (!serviceVersionInfo.Validate(this.minVersion))
            {
                return false;
            }

            if (!serviceVersionInfo.Validate(this.maxVersion))
            {
                return false;
            }

            // check that minVersion is <= maxVersion
            int comparison = this.minVersion.CompareTo(this.maxVersion);
            if (comparison == 1)
            {
                // minVersion appears to be > maxVersion
                return false;
            }

            return true;
        }

        /// <summary>
        /// Performs the work of parsing the version range string.
        /// </summary>
        /// <param name="serviceVersionInfo">service version info</param>
        /// <param name="versionRange">the version range string</param>
        /// <returns>true if the version string parses successfully</returns>
        private bool ParseVersionRange(IServiceVersionInfo serviceVersionInfo, string versionRange)
        {
            string versionPattern = @"(All)|(Min)|(Cur)|[vV](\d+).(\d+)";
            string firstVersion = null, secondVersion = null;
            int separatorPosition = versionRange.IndexOf('-');

            if (separatorPosition >= 0)
            {
                firstVersion = versionRange.Substring(0, separatorPosition);
                var match = Regex.Match(firstVersion, versionPattern);
                if (match.Success)
                {
                    bool isAll = match.Groups[1].Value.Equals(All);
                    bool isMin = match.Groups[2].Value.Equals(Min);
                    bool isCur = match.Groups[3].Value.Equals(Cur);
                    if (isAll)
                    {
                        // cannot have a version range that begins with All and a hypen that follows
                        this.parseError = true;
                        return false;
                    }
                    else if (isMin)
                    {
                        this.minVersion = serviceVersionInfo.GetMinVersion();
                    }
                    else if (isCur)
                    {
                        this.minVersion = serviceVersionInfo.GetMaxVersion();
                    }
                    else
                    {
                        int major = int.Parse(match.Groups[4].Value);
                        int minor = int.Parse(match.Groups[5].Value);
                        if (major < 0 || minor < 0)
                        {
                            this.parseError = true;
                            return false;
                        }

                        this.minVersion = new ApiVersionInfo(major, minor);
                    }
                }
                else
                {
                    this.parseError = true;
                    return false;
                }

                secondVersion = versionRange.Substring(separatorPosition + 1);
                match = Regex.Match(secondVersion, versionPattern);
                if (match.Success)
                {
                    bool isAll = match.Groups[1].Value.Equals(All);
                    bool isMin = match.Groups[2].Value.Equals(Min);
                    bool isCur = match.Groups[3].Value.Equals(Cur);
                    if (isAll)
                    {
                        // cannot have a version range that has a hypen and then All
                        this.parseError = true;
                        return false;
                    }
                    else if (isMin)
                    {
                        // cannot have a version range that has a hypen and then Min
                        this.parseError = true;
                        return false;
                    }
                    else if (isCur)
                    {
                        this.maxVersion = serviceVersionInfo.GetMaxVersion();
                    }
                    else
                    {
                        int major = int.Parse(match.Groups[4].Value);
                        int minor = int.Parse(match.Groups[5].Value);
                        if (major < 0 || minor < 0)
                        {
                            this.parseError = true;
                            return false;
                        }

                        this.maxVersion = new ApiVersionInfo(major, minor);
                    }
                }
                else
                {
                    this.parseError = true;
                    return false;
                }
            }
            else
            {
                firstVersion = versionRange;
                var match = Regex.Match(firstVersion, versionPattern);
                if (match.Success)
                {
                    bool isAll = match.Groups[1].Value.Equals(All);
                    bool isMin = match.Groups[2].Value.Equals(Min);
                    bool isCur = match.Groups[3].Value.Equals(Cur);
                    if (isAll)
                    {
                        this.minVersion = serviceVersionInfo.GetMinVersion();
                        this.maxVersion = serviceVersionInfo.GetMaxVersion();
                    }
                    else if (isMin)
                    {
                        this.minVersion = serviceVersionInfo.GetMinVersion();
                        this.maxVersion = serviceVersionInfo.GetMinVersion();
                    }
                    else if (isCur)
                    {
                        this.minVersion = serviceVersionInfo.GetMaxVersion();
                        this.maxVersion = serviceVersionInfo.GetMaxVersion();
                    }
                    else
                    {
                        int major = int.Parse(match.Groups[4].Value);
                        int minor = int.Parse(match.Groups[5].Value);
                        if (major < 0 || minor < 0)
                        {
                            this.parseError = true;
                            return false;
                        }

                        this.minVersion = new ApiVersionInfo(major, minor);
                        this.maxVersion = new ApiVersionInfo(major, minor);
                    }
                }
                else
                {
                    this.parseError = true;
                    return false;
                }
            }

            return true;
        }
    }
}