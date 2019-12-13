// <copyright file="QueueDescriptorProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System.Collections.Generic;

    /// <summary>
    /// Queue descriptor provider -- All queue descriptors are programmed here
    /// </summary>
    public static class QueueDescriptorProvider
    {
        /// <summary>
        /// Queue descriptors dictionary
        /// </summary>
        private static Dictionary<QueueIdentifier, QueueDescriptor> queues = new Dictionary<QueueIdentifier, QueueDescriptor>();

        /// <summary>
        /// Initializes static members of the <see cref="QueueDescriptorProvider"/> class.
        /// </summary>
        static QueueDescriptorProvider()
        {
            Initalize();
        }

        /// <summary>
        /// Gets queue descriptors dictionary
        /// </summary>
        public static Dictionary<QueueIdentifier, QueueDescriptor> Queues
        {
            get
            {
                return queues;
            }
        }

        /// <summary>
        /// Initialize queue descriptors
        /// </summary>
        private static void Initalize()
        {
            QueueDescriptor fanoutActivities = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.FanoutActivities.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor fanoutTopics = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.FanoutTopics.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor followingImports = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.FollowingImports.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor likes = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.Likes.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor moderation = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.Moderation.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor relationships = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.Relationships.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor reports = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.Reports.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor resizeImages = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.ResizeImages.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };
            QueueDescriptor search = new QueueDescriptor()
            {
                QueueName = QueueIdentifier.Search.ToString(),
                ServiceBusInstanceType = ServiceBusInstanceType.Default
            };

            Add(QueueIdentifier.FanoutActivities, fanoutActivities);
            Add(QueueIdentifier.FanoutTopics, fanoutTopics);
            Add(QueueIdentifier.FollowingImports, followingImports);
            Add(QueueIdentifier.Likes, likes);
            Add(QueueIdentifier.Moderation, moderation);
            Add(QueueIdentifier.Relationships, relationships);
            Add(QueueIdentifier.Reports, reports);
            Add(QueueIdentifier.ResizeImages, resizeImages);
            Add(QueueIdentifier.Search, search);
        }

        /// <summary>
        /// Add queue descriptor for queue identifier
        /// </summary>
        /// <param name="queueIdentifier">Queue identifier</param>
        /// <param name="queueDescriptor">Queue descriptor</param>
        private static void Add(QueueIdentifier queueIdentifier, QueueDescriptor queueDescriptor)
        {
            queues.Add(queueIdentifier, queueDescriptor);
        }
    }
}
