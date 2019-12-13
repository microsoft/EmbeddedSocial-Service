// <copyright file="ValidationConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using Microsoft.CTStore;

    /// <summary>
    /// Validation configuration entity class
    /// </summary>
    public class ValidationConfigurationEntity : ObjectEntity, IValidationConfigurationEntity
    {
        /// <summary>
        /// Gets or sets a value indicating whether automatic and manual validation is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic text validation is enabled
        /// </summary>
        public bool ValidateText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether automatic image validation is enabled
        /// </summary>
        public bool ValidateImages { get; set; }

        /// <summary>
        /// Gets or sets threshold for user reporting after which manual validation kicks in
        /// </summary>
        public int UserReportThreshold { get; set; }

        /// <summary>
        /// Gets or sets threshold for content reporting after which manual validation kicks in
        /// </summary>
        public int ContentReportThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mature content is allowed
        /// </summary>
        public bool AllowMatureContent { get; set; }
    }
}
