// <copyright file="KnownPolicyCodes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.AVERT
{
    using System.Collections.Generic;

    /// <summary>
    /// Policy codes from AVERT and CVS for content that is known to be benign
    /// </summary>
    /// <remarks>
    /// These code values come from the AVERT and CVS team, based on querying their GET /policies API.
    /// </remarks>
    public class KnownPolicyCodes : IPolicyCodes
    {
        /// <summary>
        /// Known policy codes - any content exhibiting these policy codes is benign
        /// </summary>
        private static List<int> known = new List<int>()
        {
            8481 // Analyzer: Experimental
        };

        /// <summary>
        /// Is content with this policy code a member of the known benign policy codes
        /// </summary>
        /// <param name="policyCode">AVERT and CVS policy code</param>
        /// <returns>true if this is a known policy code</returns>
        public bool IsViolation(int policyCode)
        {
            return known.Contains(policyCode);
        }
    }
}
