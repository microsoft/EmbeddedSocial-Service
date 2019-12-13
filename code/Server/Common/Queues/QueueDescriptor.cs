// <copyright file="QueueDescriptor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    /// <summary>
    /// Queue descriptor class
    /// </summary>
    public class QueueDescriptor
    {
        /// <summary>
        /// Gets or sets service bus instance type
        /// </summary>
        public ServiceBusInstanceType ServiceBusInstanceType { get; set; }

        /// <summary>
        /// Gets or sets queue name
        /// </summary>
        public string QueueName { get; set; }
    }
}
