// <copyright file="MinorVersionInfo.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.Versioning
{
    /// <summary>
    /// Structure that represents the range of valid minor versions for a particular major version number.
    /// </summary>
    public struct MinorVersionInfo
    {
        /// <summary>
        /// The smallest valid minor version number
        /// </summary>
        private int min;

        /// <summary>
        /// The largest valid minor version number
        /// </summary>
        private int max;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinorVersionInfo"/> struct.
        /// </summary>
        /// <param name="min">The smallest valid minor version number</param>
        /// <param name="max">The largest valid minor version number</param>
        public MinorVersionInfo(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>
        /// Gets the smallest valid minor version number
        /// </summary>
        public int Min
        {
            get { return this.min; }
        }

        /// <summary>
        /// Gets the largest valid minor version number
        /// </summary>
        public int Max
        {
            get { return this.max; }
        }
    }
}
