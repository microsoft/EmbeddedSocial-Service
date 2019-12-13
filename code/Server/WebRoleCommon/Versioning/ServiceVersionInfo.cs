//-----------------------------------------------------------------------
// <copyright file="ServiceVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements ServiceVersionInfo class, which provides information on which are the valid API version numbers for a service.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System.Collections.Generic;

    /// <summary>
    /// The <c>ServiceVersionInfo</c> class provides information about all the valid API versions supported by a service.
    /// </summary>
    public class ServiceVersionInfo : IServiceVersionInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVersionInfo"/> class
        /// </summary>
        /// <param name="firstMajorVersion">the first major version number</param>
        /// <param name="numMajorVersions">the number of major versions</param>
        /// <param name="minorVersionInfos">info about minor version</param>
        public ServiceVersionInfo(int firstMajorVersion, int numMajorVersions, MinorVersionInfo[] minorVersionInfos)
        {
            this.FirstMajorVersion = firstMajorVersion;
            this.NumMajorVersions = numMajorVersions;
            this.ValidVersions = minorVersionInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceVersionInfo"/> class
        /// </summary>
        /// <param name="serviceVersionInfo">info of version of service</param>
        public ServiceVersionInfo(IServiceVersionInfo serviceVersionInfo)
        {
            this.FirstMajorVersion = serviceVersionInfo.FirstMajorVersion;
            this.NumMajorVersions = serviceVersionInfo.NumMajorVersions;
            this.ValidVersions = serviceVersionInfo.ValidVersions;
        }

        /// <summary>
        /// Gets lowest major version number.  All major and minor version numbers must be non-negative.
        /// </summary>
        public int FirstMajorVersion { get; private set; }

        /// <summary>
        /// Gets number of major versions.
        /// </summary>
        public int NumMajorVersions { get; private set; }

        /// <summary>
        /// Gets array that records the valid minor version numbers for a given major version number.
        /// This array is always indexed by the major version number.
        /// </summary>
        public MinorVersionInfo[] ValidVersions { get; private set; }

        /// <summary>
        /// Returns the current service version number.
        /// Current is the always the maximum valid version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the current service version number</returns>
        public ApiVersionInfo GetCurrentVersion()
        {
            return this.GetMaxVersion();
        }

        /// <summary>
        /// Returns the minimum valid service version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the minimum service version number</returns>
        public ApiVersionInfo GetMinVersion()
        {
            return new ApiVersionInfo(this.FirstMajorVersion, this.ValidVersions[this.FirstMajorVersion].Min);
        }

        /// <summary>
        /// Returns the maximum service version number.
        /// </summary>
        /// <returns>An <c>ApiVersion</c> object that is the maximum service version number</returns>
        public ApiVersionInfo GetMaxVersion()
        {
            int maxMajor = this.FirstMajorVersion + (this.NumMajorVersions - 1);
            return new ApiVersionInfo(maxMajor, this.ValidVersions[maxMajor].Max);
        }

        /// <summary>
        /// Checks whether a version is valid.
        /// </summary>
        /// <param name="vers">The version to validate</param>
        /// <returns>True if <c>vers</c> is valid, false otherwise</returns>
        public bool Validate(ApiVersionInfo vers)
        {
            int maxMajor = this.FirstMajorVersion + (this.NumMajorVersions - 1);

            // first, check the major version is valid
            if (vers.MajorVersion < this.FirstMajorVersion || vers.MajorVersion > maxMajor)
            {
                return false;
            }

            // next, check the minor version number
            if (vers.MinorVersion < this.ValidVersions[vers.MajorVersion].Min ||
                vers.MinorVersion > this.ValidVersions[vers.MajorVersion].Max)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Enumerates all the valid versions in the range from start to end
        /// </summary>
        /// <param name="start">the starting version of the range</param>
        /// <param name="end">the ending version of the range</param>
        /// <returns>a list of all valid versions in the range from start to end</returns>
        public List<string> EnumerateVersions(ApiVersionInfo start, ApiVersionInfo end)
        {
            List<string> versionList = null;

            // first, check that the start and end paramenters are valid versions
            if (this.Validate(start) == false || this.Validate(end) == false)
            {
                return null;
            }

            int startMajor = start.MajorVersion;
            for (int i = start.MajorVersion; i <= end.MajorVersion; i++)
            {
                int minorStart = this.ValidVersions[i].Min;
                int minorEnd = this.ValidVersions[i].Max;

                if (i == start.MajorVersion)
                {
                    minorStart = start.MinorVersion;
                }

                if (i == end.MajorVersion)
                {
                    minorEnd = end.MinorVersion;
                }

                for (int j = minorStart; j <= minorEnd; j++)
                {
                    string version = new ApiVersionInfo(i, j).ToString();
                    if (versionList == null)
                    {
                        versionList = new List<string>();
                    }

                    versionList.Add(version);
                }
            }

            return versionList;
        }
    }
}