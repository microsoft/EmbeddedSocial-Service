// <copyright file="NotAllowedPolicyCodes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.AVERT
{
    using System.Collections.Generic;

    /// <summary>
    /// Policy codes from AVERT and CVS for content that is not allowed on Social Plus
    /// </summary>
    /// <remarks>
    /// These code values come from the AVERT and CVS team, based on email conversation.
    /// There is no programmatic way to extract the not allowed policy codes.
    /// </remarks>
    public class NotAllowedPolicyCodes : IPolicyCodes
    {
        /// <summary>
        /// Banned - any content exhibiting these policy codes is not allowed on Social Plus
        /// </summary>
        private static List<int> notAllowed = new List<int>()
        {
            21, // CSAM (Child Exploitation)
            2078, // Spam, Flooding, Malicious Content
            8406, // CSAM Needs Confirmation
            8407, // CSAM Needs Review
            8506, // Extreme Terrorism
            8530, // Extreme Terrorism Needs Confirmation
            8531 // Extreme Terrorism Needs Review
        };

        /// <summary>
        /// Is content with this policy code not allowed on Social Plus?
        /// </summary>
        /// <param name="policyCode">AVERT and CVS policy code</param>
        /// <returns>true if this content is not allowed on Social Plus</returns>
        public bool IsViolation(int policyCode)
        {
            return notAllowed.Contains(policyCode);
        }
    }
}