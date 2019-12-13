// <copyright file="ContainerDescriptor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Tables
{
    using System.Collections.Generic;

    /// <summary>
    /// Container descriptor class
    /// </summary>
    public class ContainerDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerDescriptor" /> class
        /// </summary>
        public ContainerDescriptor()
        {
            this.Tables = new Dictionary<TableIdentifier, TableDescriptor>();
        }

        /// <summary>
        /// Gets or sets container name
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets container initial
        /// </summary>
        public string ContainerInitial { get; set; }

        /// <summary>
        /// Gets or sets tables in the container
        /// </summary>
        public Dictionary<TableIdentifier, TableDescriptor> Tables { get; set; }

        /// <summary>
        /// Gets or sets Azure storage instance type
        /// </summary>
        public AzureStorageInstanceType AzureStorageInstanceType { get; set; }

        /// <summary>
        /// Gets or sets <c>Redis</c> instance type
        /// </summary>
        public RedisInstanceType RedisInstanceType { get; set; }
    }
}
