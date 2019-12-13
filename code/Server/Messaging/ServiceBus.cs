// <copyright file="ServiceBus.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messaging
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Service bus class
    /// </summary>
    /// <remarks>
    /// Used to manipulate the service bus namespace
    /// </remarks>
    public class ServiceBus
    {
        /// <summary>
        /// Namespace manager
        /// </summary>
        private readonly NamespaceManager nsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBus"/> class
        /// </summary>
        /// <param name="connectionString">connection string</param>
        public ServiceBus(string connectionString)
        {
            this.nsManager = NamespaceManager.CreateFromConnectionString(connectionString);
        }

        /// <summary>
        /// Create queue async
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>Create queue task</returns>
        public async Task CreateQueueAsync(string queueName)
        {
            // check to see the specified queue name exists
            if (!await this.nsManager.QueueExistsAsync(queueName))
            {
                // if not, use the namespace manager to create the queue
                QueueDescription queueDescription = new QueueDescription(queueName)
                {
                    EnablePartitioning = true
                };

                try
                {
                    await this.nsManager.CreateQueueAsync(queueDescription);
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    // if the queue already exists, then our work is done.
                    // do not expose this exception to the caller
                }
            }
        }

        /// <summary>
        /// Delete queue async
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>Delete queue task</returns>
        public async Task DeleteQueueAsync(string queueName)
        {
            if (this.nsManager.QueueExists(queueName))
            {
                try
                {
                    await this.nsManager.DeleteQueueAsync(queueName);
                }
                catch (MessagingEntityNotFoundException)
                {
                    // if the queue is already deleted, then our work is done.
                    // do not expose this exception to the caller
                }
            }
        }

        /// <summary>
        /// List the names of all queues in the namespace
        /// </summary>
        /// <returns>list of queue names</returns>
        public async Task<IList<string>> ListQueues()
        {
            var list = await this.nsManager.GetQueuesAsync();
            var result = (from x in list select x.Path).ToList();
            return result;
        }
    }
}
