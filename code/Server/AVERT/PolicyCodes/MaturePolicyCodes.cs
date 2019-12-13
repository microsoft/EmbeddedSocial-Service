// <copyright file="MaturePolicyCodes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.AVERT
{
    using System.Collections.Generic;

    /// <summary>
    /// Policy codes from AVERT and CVS that indicate mature content.
    /// </summary>
    /// <remarks>
    /// These code values come from the AVERT and CVS team, based on email conversation.
    /// There is no programmatic way to extract the mature-specific policy codes.
    /// </remarks>
    public class MaturePolicyCodes : IPolicyCodes
    {
        /// <summary>
        /// Mature - any content exhibiting these policy codes is considered adult in nature
        /// </summary>
        private static List<int> mature = new List<int>()
        {
            46,   // Excessive or gratuitous profanity and vulgarity are prohibited
            1067, // Adult & Sexual
            1068, // Alcohol, Tobacco, Drugs
            1069, // Profanity & Vulgarity
            1070, // Violence, Weapons & Gore
        };

        /// <summary>
        /// Is content with this policy code considered mature content?
        /// </summary>
        /// <param name="policyCode">AVERT and CVS policy code</param>
        /// <returns>true if this content is for mature audiences</returns>
        public bool IsViolation(int policyCode)
        {
            return mature.Contains(policyCode);
        }
    }
}