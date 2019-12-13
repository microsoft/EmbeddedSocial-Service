// <copyright file="QueueManager.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Queue manager
    /// </summary>
    public class QueueManager : IQueueManager
    {
        /// <summary>
        /// Cached queue objects
        /// </summary>
        private static ConcurrentDictionary<string, Queue> cachedQueueObjects = new ConcurrentDictionary<string, Queue>();

        /// <summary>
        /// Connection string provider
        /// </summary>
        private readonly IConnectionStringProvider connectionStringProvider;

        /// <summary>
        /// Client-side batching interval to use for Service Bus QueueClients, in milliseconds
        /// </summary>
        private int batchIntervalMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueManager"/> class.
        /// </summary>
        /// <param name="connectionStringProvider">connection string provider</param>
        /// <param name="batchIntervalMs">client-side batching interval, in milliseconds</param>
        public QueueManager(IConnectionStringProvider connectionStringProvider, int batchIntervalMs)
        {
            this.connectionStringProvider = connectionStringProvider;
            this.batchIntervalMs = batchIntervalMs;
        }

        /// <summary>
        /// Get queue from queue identifier
        /// </summary>
        /// <param name="queueIdentifier">Queue identifier</param>
        /// <returns>Messaging queue</returns>
        public async Task<Queue> GetQueue(QueueIdentifier queueIdentifier)
        {
            QueueDescriptor queueDescriptor = QueueDescriptorProvider.Queues[queueIdentifier];
            string serviceBusConnectionString = await this.connectionStringProvider.GetServiceBusConnectionString(queueDescriptor.ServiceBusInstanceType);
            string queueName = queueDescriptor.QueueName;
            string uniqueQueueIdentity = string.Join(":", serviceBusConnectionString, queueName);

            // cachedQueueObjects is a thread-safe dictionary (ConcurrentDictionary). If uniqueQueueIdentity is not present
            // in cachedStoreObjects, try adding it. Since GetQueue can be called concurrently by
            // different threads, it is possible for two (or more) threads to attempt inserting uniqueQueueIdentity
            // concurrently in the cachedQueueObjects. That's ok, because the call to TryAdd is guaranteed to be thread-safe.
            // One of the threads will not be able to insert (i.e., TryAdd will return false), but the code will happily execute
            // and fall through to the return statement.
            // This code makes no use of locking on the common path (i.e., reads of cachedStoreObjects).
            if (!cachedQueueObjects.ContainsKey(uniqueQueueIdentity))
            {
                ServiceBusQueue serviceBusQueue = await ServiceBusQueue.Create(serviceBusConnectionString, queueName, this.batchIntervalMs);
                Queue queue = new Queue(serviceBusQueue);
                cachedQueueObjects.TryAdd(uniqueQueueIdentity, queue);
            }

            return cachedQueueObjects[uniqueQueueIdentity];
        }
    }
}
