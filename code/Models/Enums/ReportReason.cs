// <copyright file="ReportReason.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Models
{
    /// <summary>
    /// Reasons that a user can give for reporting content or another user.
    /// </summary>
    /// <remarks>
    /// The enum strings below must match the enum strings in Microsoft.Ops.Avert.Client.DataContracts.AbuseCategory.
    /// </remarks>
    public enum ReportReason
    {
        /// <summary>
        /// Threats, Cyberbullying or Harassment.
        /// </summary>
        ThreatsCyberbullyingHarassment = 0,

        /// <summary>
        /// Child endangerment or exploitation.
        /// </summary>
        ChildEndangermentExploitation = 1,

        /// <summary>
        /// Offensive content.
        /// </summary>
        OffensiveContent = 2,

        /// <summary>
        /// Virus Spyware and/or Malware.
        /// </summary>
        VirusSpywareMalware = 3,

        /// <summary>
        /// Content infrigement.
        /// </summary>
        ContentInfringement = 4,

        /// <summary>
        /// Other category.
        /// </summary>
        Other = 5,

        /// <summary>
        /// No abusive category selected.
        /// </summary>
        None = 6
    }
}
