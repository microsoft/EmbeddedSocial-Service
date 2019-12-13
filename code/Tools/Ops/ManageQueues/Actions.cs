// <copyright file="Actions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Tools.ManageQueues
{
    using System;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Implements manual control of service bus queues
    /// </summary>
    public class Actions
    {
        /// <summary>
        /// List the names of the queues in a namespace
        /// </summary>
        /// <param name="sb">service bus</param>
        /// <returns>list queue names task</returns>
        public async Task ListQueues(ServiceBus sb)
        {
            var queueNames = await sb.ListQueues();

            if (queueNames.Count > 0)
            {
                Console.WriteLine("Queue names:");
            }
            else
            {
                Console.WriteLine("No queues found in service bus namespace.");
            }

            foreach (var qn in queueNames)
            {
                Console.WriteLine($"{qn}");
            }
        }

        /// <summary>
        /// Show the message stats for a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="sbq">service bus queue</param>
        /// <returns>queue stats task</returns>
        public async Task QueueStats(string queueName, ServiceBusQueue sbq)
        {
            var messageCount = await sbq.MessageCount();
            var deadLetterMessageCount = await sbq.DeadLetterMessageCount();

            Console.WriteLine($"Number of messages in {queueName} queue: Active = {messageCount}, DeadLetter = {deadLetterMessageCount}");
        }

        /// <summary>
        /// Show the messages in a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="sbq">service bus queue</param>
        /// <returns>show messages task</returns>
        public async Task ShowMessages(string queueName, ServiceBusQueue sbq)
        {
            Console.WriteLine($"Messages in {queueName} queue:");
            int messageCount = (int)await sbq.MessageCount();
            Console.WriteLine($"Number of queued messages = {messageCount}");
            var msgList = await sbq.PeekBatchAsync(messageCount);
            var remaining = messageCount - msgList.Count;

            // Because our queues are partitioned, peek will not see all messages
            // on the first attempt. So we call peek repeatedly until all messages are seen.
            while (remaining > 0)
            {
                var newMsgList = await sbq.PeekBatchAsync(remaining);
                if (newMsgList.Count == 0)
                {
                    break;
                }

                remaining -= newMsgList.Count;
                foreach (var msg in newMsgList)
                {
                    msgList.Add(msg);
                }
            }

            foreach (var msg in msgList)
            {
                Console.WriteLine($"Queue message: enqueued time = {msg.EnqueuedTime}, dequeue count = {msg.DequeueCount}");
                this.ShowMessageContents(msg);
            }
        }

        /// <summary>
        /// Show the messages in the dead letter queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="sbq">service bus queue</param>
        /// <returns>show messages task</returns>
        public async Task ShowDeadLetterMessages(string queueName, ServiceBusQueue sbq)
        {
            Console.WriteLine($"Messages in dead letter queue for {queueName}:");
            var deadLetterMessageCount = (int)await sbq.DeadLetterMessageCount();
            Console.WriteLine($"Number of queued messages = {deadLetterMessageCount}");
            var msgList = await sbq.PeekDeadLetterBatchAsync(deadLetterMessageCount);
            var remaining = deadLetterMessageCount - msgList.Count;

            // Because our queues are partitioned, peek will not see all messages
            // on the first attempt. So we call peek repeatedly until all messages are seen.
            while (remaining > 0)
            {
                var newMsgList = await sbq.PeekDeadLetterBatchAsync(remaining);
                if (newMsgList.Count == 0)
                {
                    break;
                }

                remaining -= newMsgList.Count;
                foreach (var msg in newMsgList)
                {
                    msgList.Add(msg);
                }
            }

            foreach (var msg in msgList)
            {
                Console.WriteLine($"Queue message: time = {msg.EnqueuedTime}, dequeue count = {msg.DequeueCount}");
                this.ShowMessageContents(msg);
            }
        }

        /// <summary>
        /// Delete a queue message from the dead letter queue given its sequence number
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="sbq">service bus queue</param>
        /// <param name="seqNum">sequence number</param>
        /// <returns>delete message task</returns>
        public async Task DeleteDeadLetterMessage(string queueName, ServiceBusQueue sbq, long seqNum)
        {
            var deadLetterMessageCount = await sbq.DeadLetterMessageCount();
            var remaining = deadLetterMessageCount;
            bool deleted = false;

            while (remaining > 0 && deleted == false)
            {
                var msgList = await sbq.ReceiveDeadLetterBatchAsync((int)remaining);
                remaining -= msgList.Count;
                foreach (var msg in msgList)
                {
                    if (msg.SequenceNumber == seqNum)
                    {
                        await sbq.CompleteAsync(msg);
                        deleted = true;
                        break;
                    }
                    else
                    {
                        await sbq.AbandonAsync(msg);
                    }
                }
            }
        }

        /// <summary>
        /// Print the contents of a message
        /// </summary>
        /// <remarks>
        /// Each time a new message type is added in Server\Common\Messages, we must add an additional clause to this method.
        /// </remarks>
        /// <param name="msg">message to print</param>
        private void ShowMessageContents(IQueueMessage msg)
        {
            if (msg is ContentModerationMessage contentModerationMessage)
            {
                Console.WriteLine("ContentModerationMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(contentModerationMessage));
            }
            else if (msg is FanoutActivityMessage fanoutActivityMessage)
            {
                Console.WriteLine("FanoutActivityMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(fanoutActivityMessage));
            }
            else if (msg is FanoutTopicActivityMessage fanoutTopicActivityMessage)
            {
                Console.WriteLine("FanoutTopicActivityMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(fanoutTopicActivityMessage));
            }
            else if (msg is FanoutTopicMessage fanoutTopicMessage)
            {
                Console.WriteLine("FanoutTopicMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(fanoutTopicMessage));
            }
            else if (msg is FollowingImportMessage followingImportMessage)
            {
                Console.WriteLine("FollowingImportMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(followingImportMessage));
            }
            else if (msg is ImageModerationMessage imageModerationMessage)
            {
                Console.WriteLine("ImageModerationMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(imageModerationMessage));
            }
            else if (msg is LikeMessage likeMessage)
            {
                Console.WriteLine("LikeMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(likeMessage));
            }
            else if (msg is RelationshipMessage relationshipMessage)
            {
                Console.WriteLine("RelationshipMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(relationshipMessage));
            }
            else if (msg is ReportMessage reportMessage)
            {
                Console.WriteLine("ReportMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(reportMessage));
            }
            else if (msg is SearchIndexTopicMessage searchIndexTopicMessage)
            {
                Console.WriteLine("SearchIndexTopicMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(searchIndexTopicMessage));
            }
            else if (msg is SearchIndexUserMessage searchIndexUserMessage)
            {
                Console.WriteLine("SearchIndexUserMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(searchIndexUserMessage));
            }
            else if (msg is SearchRemoveTopicMessage searchRemoveTopicMessage)
            {
                Console.WriteLine("SearchRemoveTopicMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(searchRemoveTopicMessage));
            }
            else if (msg is SearchRemoveUserMessage searchRemoveUserMessage)
            {
                Console.WriteLine("SearchRemoveUserMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(searchRemoveUserMessage));
            }
            else if (msg is UserModerationMessage userModerationMessage)
            {
                Console.WriteLine("UserModerationMessage:");
                Console.WriteLine(JsonConvert.SerializeObject(userModerationMessage));
            }
            else
            {
                Console.WriteLine($"Unknown message type: {msg.GetType().Name}");
            }
        }
    }
}
