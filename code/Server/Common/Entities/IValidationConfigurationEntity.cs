// <copyright file="IValidationConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    /// <summary>
    /// Validation configuration entity interface
    /// </summary>
    public interface IValidationConfigurationEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic and manual validation is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic text validation is enabled
        /// </summary>
        bool ValidateText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic image validation is enabled
        /// </summary>
        bool ValidateImages { get; set; }

        /// <summary>
        /// Gets or sets threshold for user reporting after which manual validation kicks in
        /// </summary>
        int UserReportThreshold { get; set; }

        /// <summary>
        /// Gets or sets threshold for content reporting after which manual validation kicks in
        /// </summary>
        int ContentReportThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mature content is allowed
        /// </summary>
        bool AllowMatureContent { get; set; }
    }
}
