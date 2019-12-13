// <copyright file="IMetricsConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Metrics
{
    /// <summary>
    /// Interface to configure the metrics provider
    /// </summary>
    public interface IMetricsConfig
    {
        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        /// <value>The name of the tenant.</value>
        string TenantName { get; }

        /// <summary>
        /// Gets the name of the instance.
        /// </summary>
        /// <value>The name of the instance.</value>
        string InstanceName { get; }

        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        /// <value>The name of the role.</value>
        string RoleName { get; }
    }
}
