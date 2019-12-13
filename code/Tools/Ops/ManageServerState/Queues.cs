// <copyright file="Queues.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageServerState
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Server.Messaging;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// portion of Program class that deals with creating and deleting service bus queues
    /// </summary>
    public partial class Program
    {
        /// <summary>
        /// Provision service bus queues
        /// </summary>
        /// <param name="serviceBusConnectionString">Service bus connection string</param>
        /// <returns>Provision task</returns>
        private static async Task ProvisionServiceBusQueues(string serviceBusConnectionString)
        {
            Console.WriteLine("Creating service bus queues...");
            foreach (QueueIdentifier queueIdentifier in Enum.GetValues(typeof(QueueIdentifier)))
            {
                if (!QueueDescriptorProvider.Queues.ContainsKey(queueIdentifier))
                {
                    Console.WriteLine("  " + queueIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                QueueDescriptor queueDescriptor = QueueDescriptorProvider.Queues[queueIdentifier];
                ServiceBus serviceBus = new ServiceBus(serviceBusConnectionString);
                await serviceBus.CreateQueueAsync(queueDescriptor.QueueName);
                Console.WriteLine("  " + queueIdentifier.ToString() + " - Queue Provisioned");
            }
        }

        /// <summary>
        /// Delete all the service bus queues
        /// </summary>
        /// <param name="serviceBusConnectionString">connection string to service bus</param>
        /// <returns>delete task</returns>
        private static async Task DeleteServiceBusQueues(string serviceBusConnectionString)
        {
            // Delete service bus queues.
            // TODO: Investigate the delete function for service bus queue. We've had problems with it in the past.
            Console.WriteLine("Deleting service bus queues...");
            foreach (QueueIdentifier queueIdentifier in Enum.GetValues(typeof(QueueIdentifier)))
            {
                if (!QueueDescriptorProvider.Queues.ContainsKey(queueIdentifier))
                {
                    Console.WriteLine("  " + queueIdentifier.ToString() + " - Descriptor not found");
                    continue;
                }

                QueueDescriptor queueDescriptor = QueueDescriptorProvider.Queues[queueIdentifier];
                ServiceBus serviceBus = new ServiceBus(serviceBusConnectionString);
                await serviceBus.DeleteQueueAsync(queueDescriptor.QueueName);
                Console.WriteLine("  " + queueIdentifier.ToString() + " - Queue Deleted");
            }
        }
    }
}
