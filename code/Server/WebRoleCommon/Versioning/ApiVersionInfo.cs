//-----------------------------------------------------------------------
// <copyright file="ApiVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <summary>
// Implements the API Version class.
// </summary>
//-----------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The <c>ApiVersion</c> class represents a version as a pair of integers: (<c>MajorVersion</c>, <c>MinorVersion</c>).
    /// </summary>
    public class ApiVersionInfo : IEquatable<ApiVersionInfo>, IComparable<ApiVersionInfo>
    {
        /// <summary>
        /// major version number
        /// </summary>
        private int majorVersion;

        /// <summary>
        /// minor version number
        /// </summary>
        private int minorVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionInfo"/> class.
        /// </summary>
        /// <param name="major">major version number</param>
        /// <param name="minor">minor version number</param>
        public ApiVersionInfo(int major, int minor)
        {
            this.majorVersion = major;
            this.minorVersion = minor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionInfo"/> class
        /// using the version specified in an HTTP Request URI, such as "v0.4/"
        /// </summary>
        /// <param name="versionSegmentFromUri">version segment string from the URI</param>
        public ApiVersionInfo(string versionSegmentFromUri)
        {
            // check for invalid string
            if (string.IsNullOrWhiteSpace(versionSegmentFromUri))
            {
                throw new ArgumentNullException("versionSegmentFromUri");
            }

            // remove slashes if they exist
            versionSegmentFromUri = versionSegmentFromUri.Replace("/", string.Empty);

            // remove initial "v" if it exists
            int indexOfV = versionSegmentFromUri.IndexOf("v");
            if (indexOfV == 0)
            {
                versionSegmentFromUri = versionSegmentFromUri.Remove(indexOfV, 1);
            }

            // get the minor & major version numbers
            string[] versionNumbers = versionSegmentFromUri.Split('.');
            if (versionNumbers.Length >= 2)
            {
                this.majorVersion = int.Parse(versionNumbers[0]);
                this.minorVersion = int.Parse(versionNumbers[1]);
            }
            else
            {
                throw new ArgumentException("versionSegmentFromUri: " + versionSegmentFromUri + "is not valid");
            }
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public int MajorVersion
        {
            get { return this.majorVersion; }
        }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public int MinorVersion
        {
            get { return this.minorVersion; }
        }

        /// <summary>
        /// Implements the Equals comparison.
        /// </summary>
        /// <param name="other">The other <c>ApiVersion</c> we are comparing with.</param>
        /// <returns>True if the other <c>ApiVersion</c> is equal to this, false otherwise.</returns>
        public bool Equals(ApiVersionInfo other)
        {
            if (other == null)
            {
                return false;
            }
            else
            {
                return this.majorVersion == other.majorVersion && this.minorVersion == other.minorVersion;
            }
        }

        /// <summary>
        /// Implements equals for other objects
        /// </summary>
        /// <param name="obj">The other object we are comparing with.</param>
        /// <returns>True if the <c>obj</c> <c>ApiVersion</c> is equal to this, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            ApiVersionInfo vers = obj as ApiVersionInfo;
            if (vers != null)
            {
                return this.Equals(vers);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Converts an <c>ApiVersion</c> object into a string.
        /// </summary>
        /// <returns>a string</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "v{0}.{1}", this.majorVersion, this.minorVersion);
        }

        /// <summary>
        /// Calculates a hash code for an <c>ApiVersion</c> object.
        /// </summary>
        /// <returns>a hash code</returns>
        public override int GetHashCode()
        {
            return this.majorVersion.GetHashCode() ^ this.minorVersion.GetHashCode();
        }

        /// <summary>
        /// Implements comparison with another <c>ApiVersion</c> object.
        /// </summary>
        /// <param name="other">The other <c>ApiVersion</c> object to compare with</param>
        /// <returns>1 if greater than other, 0 if equal, and -1 if less than other</returns>
        public int CompareTo(ApiVersionInfo other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null)
            {
                return 1;
            }

            if (this.majorVersion > other.majorVersion)
            {
                return 1;
            }
            else if (this.majorVersion < other.majorVersion)
            {
                return -1;
            }
            else
            {
                // if we reach here, then (this.majorVersion == other.majorVersion)
                if (this.minorVersion == other.minorVersion)
                {
                    return 0;
                }
                else if (this.minorVersion > other.minorVersion)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}