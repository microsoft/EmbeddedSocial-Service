// <copyright file="ServiceBusQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Service bus queue class
    /// </summary>
    public class ServiceBusQueue
    {
        /// <summary>
        /// Message type property name
        /// </summary>
        private readonly string messageTypePropertyName = "messageType";

        /// <summary>
        /// Suffix for the dead letter queue name
        /// </summary>
        private readonly string deadLetterQueueSuffix = "/$DeadLetterQueue";

        /// <summary>
        /// Namespace manager
        /// </summary>
        private readonly NamespaceManager nsManager;

        /// <summary>
        /// Connection string to Service bus queue
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Queue name
        /// </summary>
        private string queueName;

        /// <summary>
        /// Client-side batching interval in milliseconds
        /// </summary>
        private int batchIntervalMs;

        /// <summary>
        /// Queue client
        /// </summary>
        private QueueClient queueClient;

        /// <summary>
        /// Dead letter queue client
        /// </summary>
        private QueueClient deadLetterQueueClient;

        /// <summary>
        /// Initialization task for work not performed in the constructor
        /// </summary>
        private Task initialization;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueue"/> class
        /// </summary>
        /// <param name="connectionString">Service bus connection string</param>
        /// <param name="queueName">Queue name</param>
        /// <param name="batchIntervalMs">Client-side batching interval, in milliseconds</param>
        private ServiceBusQueue(string connectionString, string queueName, int batchIntervalMs)
        {
            this.connectionString = connectionString;
            this.queueName = queueName;
            this.batchIntervalMs = batchIntervalMs;
            this.nsManager = NamespaceManager.CreateFromConnectionString(this.connectionString);
        }

        /// <summary>
        /// Static factory method ensures that async init is completed before the service bus queue object
        /// is available to clients
        /// </summary>
        /// <param name="connectionString">Service bus connection string</param>
        /// <param name="queueName">Queue name</param>
        /// <param name="batchIntervalMs">Client-side batching interval, in milliseconds</param>
        /// <returns>An initialized service bus queue</returns>
        public static async Task<ServiceBusQueue> Create(string connectionString, string queueName, int batchIntervalMs)
        {
            var sbq = new ServiceBusQueue(connectionString, queueName, batchIntervalMs);
            sbq.initialization = sbq.InitializeAsync();
            await sbq.initialization;
            return sbq;
        }

        /// <summary>
        /// Gets the number of active messages in the queue
        /// </summary>
        /// <returns>number of active messages in the queue</returns>
        public async Task<long> MessageCount()
        {
            var queue = await this.nsManager.GetQueueAsync(this.queueName);
            return queue.MessageCountDetails.ActiveMessageCount;
        }

        /// <summary>
        /// Gets the number of messages in the dead letter queue
        /// </summary>
        /// <returns>number of messages in the dead letter queue</returns>
        public async Task<long> DeadLetterMessageCount()
        {
            var queue = await this.nsManager.GetQueueAsync(this.queueName);
            return queue.MessageCountDetails.DeadLetterMessageCount;
        }

        /// <summary>
        /// Send message async
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <param name="delay">Initial visibility delay</param>
        /// <returns>Send async task</returns>
        public async Task SendAsync(QueueMessage message, TimeSpan? delay = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (this.queueClient.IsClosed)
            {
                await this.CreateQueueClient();
            }

            BrokeredMessage brokeredMessage = new BrokeredMessage(message);
            if (delay.HasValue)
            {
                brokeredMessage.ScheduledEnqueueTimeUtc = DateTime.UtcNow.Add(delay.Value);
            }

            brokeredMessage.Properties[this.messageTypePropertyName] = message.GetType().AssemblyQualifiedName;
            await this.queueClient.SendAsync(brokeredMessage);
        }

        /// <summary>
        /// Receive message async
        /// </summary>
        /// <returns>Queue message</returns>
        public async Task<IQueueMessage> ReceiveAsync()
        {
            if (this.queueClient.IsClosed)
            {
                await this.CreateQueueClient();
            }

            var brokeredMessage = await this.queueClient.ReceiveAsync();
            return this.GetQueueMessage(brokeredMessage);
        }

        /// <summary>
        /// Receive messages in a batch
        /// </summary>
        /// <param name="count">Count of messages</param>
        /// <returns>Queue messages</returns>
        public async Task<IList<IQueueMessage>> ReceiveBatchAsync(int count)
        {
            if (this.queueClient.IsClosed)
            {
                await this.CreateQueueClient();
            }

            var brokeredMessages = await this.queueClient.ReceiveBatchAsync(count);
            List<IQueueMessage> queueMessages = new List<IQueueMessage>();
            foreach (var brokeredMessage in brokeredMessages)
            {
                QueueMessage queueMessage = this.GetQueueMessage(brokeredMessage);
                if (queueMessage != null)
                {
                    queueMessages.Add(queueMessage);
                }
            }

            return queueMessages;
        }

        /// <summary>
        /// Receive dead letter messages in a batch
        /// </summary>
        /// <param name="count">Count of messages</param>
        /// <returns>Queue messages</returns>
        public async Task<IList<IQueueMessage>> ReceiveDeadLetterBatchAsync(int count)
        {
            if (this.deadLetterQueueClient.IsClosed)
            {
                await this.CreateDeadLetterQueueClient();
            }

            var brokeredMessages = await this.deadLetterQueueClient.ReceiveBatchAsync(count);
            List<IQueueMessage> queueMessages = new List<IQueueMessage>();
            foreach (var brokeredMessage in brokeredMessages)
            {
                QueueMessage queueMessage = this.GetQueueMessage(brokeredMessage);
                if (queueMessage != null)
                {
                    queueMessages.Add(queueMessage);
                }
            }

            return queueMessages;
        }

        /// <summary>
        /// Peek messages in a batch
        /// </summary>
        /// <param name="count">Count of messages</param>
        /// <returns>Queue messages</returns>
        public async Task<IList<IQueueMessage>> PeekBatchAsync(int count)
        {
            if (this.queueClient.IsClosed)
            {
                await this.CreateQueueClient();
            }

            var brokeredMessages = await this.queueClient.PeekBatchAsync(count);
            List<IQueueMessage> queueMessages = new List<IQueueMessage>();
            foreach (var brokeredMessage in brokeredMessages)
            {
                QueueMessage queueMessage = this.GetQueueMessage(brokeredMessage);
                if (queueMessage != null)
                {
                    queueMessages.Add(queueMessage);
                }
            }

            return queueMessages;
        }

        /// <summary>
        /// Peek dead letter messages in a batch
        /// </summary>
        /// <param name="count">Count of messages</param>
        /// <returns>Queue messages</returns>
        public async Task<IList<IQueueMessage>> PeekDeadLetterBatchAsync(int count)
        {
            if (this.deadLetterQueueClient.IsClosed)
            {
                await this.CreateDeadLetterQueueClient();
            }

            var brokeredMessages = await this.deadLetterQueueClient.PeekBatchAsync(count);
            List<IQueueMessage> queueMessages = new List<IQueueMessage>();
            foreach (var brokeredMessage in brokeredMessages)
            {
                QueueMessage queueMessage = this.GetQueueMessage(brokeredMessage);
                if (queueMessage != null)
                {
                    queueMessages.Add(queueMessage);
                }
            }

            return queueMessages;
        }

        /// <summary>
        /// Complete message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Complete async task</returns>
        public async Task CompleteAsync(IQueueMessage message)
        {
            QueueMessage msg = message as QueueMessage;
            BrokeredMessage brokeredMessage = msg.SystemMessage as BrokeredMessage;
            await brokeredMessage.CompleteAsync();
        }

        /// <summary>
        /// Abandon message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Abandon async task</returns>
        public async Task AbandonAsync(IQueueMessage message)
        {
            QueueMessage msg = message as QueueMessage;
            BrokeredMessage brokeredMessage = msg.SystemMessage as BrokeredMessage;
            await brokeredMessage.AbandonAsync();
        }

        /// <summary>
        /// Async initialization code.  This method is invoked by the static factory method after the constructor.
        /// </summary>
        /// <returns>initialization task</returns>
        private async Task InitializeAsync()
        {
            await this.CreateQueueClient();
            await this.CreateDeadLetterQueueClient();
        }

        /// <summary>
        /// Creates a new queue client
        /// </summary>
        /// <returns>create queue client task</returns>
        private async Task CreateQueueClient()
        {
            var queueName = this.queueName;
            var receiveMode = ReceiveMode.PeekLock;

            var messagingFactory = await this.CreateMessagingFactory();
            this.queueClient = messagingFactory.CreateQueueClient(queueName, receiveMode);
        }

        /// <summary>
        /// Creates a new dead letter queue client
        /// </summary>
        /// <returns>create dead letter queue client task</returns>
        private async Task CreateDeadLetterQueueClient()
        {
            var queueName = this.queueName;
            var receiveMode = ReceiveMode.PeekLock;

            var messagingFactory = await this.CreateMessagingFactory();
            var deadLetterQueueName = queueName + this.deadLetterQueueSuffix;
            this.deadLetterQueueClient = messagingFactory.CreateQueueClient(deadLetterQueueName, receiveMode);
        }

        /// <summary>
        /// Helper function to create a messaging factory
        /// </summary>
        /// <returns>create messaging factory task</returns>
        private async Task<MessagingFactory> CreateMessagingFactory()
        {
            int batchIntervalMs = this.batchIntervalMs;
            string[] connectionStringSplit = this.connectionString.Split(';');
            string endpoint = null;
            string keyName = null;
            string keyValue = null;

            foreach (string str in connectionStringSplit)
            {
                if (str.StartsWith("Endpoint="))
                {
                    endpoint = str.Substring("Endpoint=".Length);
                }
                else if (str.StartsWith("SharedAccessKeyName="))
                {
                    keyName = str.Substring("SharedAccessKeyName=".Length);
                }
                else if (str.StartsWith("SharedAccessKey="))
                {
                    keyValue = str.Substring("SharedAccessKey=".Length);
                }
            }

            if (endpoint == null || keyName == null || keyValue == null)
            {
                throw new ArgumentException($"Invalid connection string: endpoint = {endpoint}, keyName = {keyName}, keyValue = {keyValue}");
            }

            TimeSpan batchInterval = TimeSpan.Zero;
            if (batchIntervalMs > 0)
            {
                batchInterval = TimeSpan.FromMilliseconds(batchIntervalMs);
            }

            TokenProvider credentials = TokenProvider.CreateSharedAccessSignatureTokenProvider(keyName, keyValue);
            MessagingFactorySettings mfs = new MessagingFactorySettings()
            {
                TokenProvider = credentials
            };
            mfs.NetMessagingTransportSettings.BatchFlushInterval = batchInterval;
            MessagingFactory messagingFactory = await MessagingFactory.CreateAsync(endpoint, mfs);

            return messagingFactory;
        }

        /// <summary>
        /// Get queue message from brokered message
        /// </summary>
        /// <param name="brokeredMessage">Brokered message</param>
        /// <returns>Queue message</returns>
        private QueueMessage GetQueueMessage(BrokeredMessage brokeredMessage)
        {
            if (brokeredMessage == null)
            {
                return null;
            }

            var messageBodyType = Type.GetType(brokeredMessage.Properties[this.messageTypePropertyName].ToString());
            if (messageBodyType == null)
            {
                return null;
            }

            MethodInfo method = typeof(BrokeredMessage).GetMethod("GetBody", new Type[] { });
            MethodInfo generic = method.MakeGenericMethod(messageBodyType);
            var messageBody = generic.Invoke(brokeredMessage, null);

            QueueMessage queueMessage = messageBody as QueueMessage;
            queueMessage.EnqueuedTime = brokeredMessage.EnqueuedTimeUtc;
            queueMessage.DequeueCount = brokeredMessage.DeliveryCount;
            queueMessage.SequenceNumber = brokeredMessage.SequenceNumber;
            queueMessage.SystemMessage = brokeredMessage;

            return queueMessage;
        }
    }
}
