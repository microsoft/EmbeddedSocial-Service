// <copyright file="IPolicyCodes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.AVERT
{
    /// <summary>
    /// Interface for accessing policy codes from AVERT and CVS
    /// </summary>
    public interface IPolicyCodes
    {
        /// <summary>
        /// Checks if a specified policy code is in violation
        /// </summary>
        /// <param name="policyCode">policy code</param>
        /// <returns>true if there is a match</returns>
        bool IsViolation(int policyCode);
    }
}