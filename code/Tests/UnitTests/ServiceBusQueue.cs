// <copyright file="ServiceBusQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace SocialPlus.UnitTests
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Contains unit tests of Service Bus Queue
    /// </summary>
    [TestClass]
    public class ServiceBusQueue
    {
        /// <summary>
        /// Name of test queue
        /// </summary>
        private readonly string queueName = "TestQueue";

        /// <summary>
        /// Service bus
        /// </summary>
        private readonly ServiceBus sb;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueue"/> class.
        /// </summary>
        public ServiceBusQueue()
        {
            this.sb = new ServiceBus(ConfigurationManager.AppSettings["ServiceBusConnectionString"]);
        }

        /// <summary>
        /// enqueue a message and then de-queue it
        /// </summary>
        /// <returns>test task</returns>
        [TestMethod]
        public async Task EnqueueDequeue()
        {
            // create a test queue
            Queue testQueue = await this.CreateQueue();

            // send a test message
            TestQueueMessage sendMessage = await this.SendMessage(testQueue);

            // receive it
            TestQueueMessage recvMessage = (TestQueueMessage)await testQueue.ReceiveAsync();

            // compare it
            Assert.IsTrue(sendMessage.Equals(recvMessage));

            // delete message
            await testQueue.CompleteAsync(recvMessage);

            // delete queue
            await this.sb.DeleteQueueAsync(this.queueName);
        }

        /// <summary>
        /// enqueue a message and then abandon it and wait for it to re-appear
        /// </summary>
        /// <returns>test task</returns>
        [TestMethod]
        public async Task EnqueueAbandonDequeue()
        {
            // create a test queue
            Queue testQueue = await this.CreateQueue();

            // send a test message
            TestQueueMessage sendMessage = await this.SendMessage(testQueue);

            // receive it
            TestQueueMessage recvMessage = (TestQueueMessage)await testQueue.ReceiveAsync();

            // compare it
            Assert.IsTrue(sendMessage.Equals(recvMessage));

            // abandon it
            await testQueue.AbandonAsync(recvMessage);

            // receive it again
            TestQueueMessage recvMessage2 = (TestQueueMessage)await testQueue.ReceiveAsync();

            // compare it
            Assert.IsTrue(sendMessage.Equals(recvMessage2));

            // delete message
            await testQueue.CompleteAsync(recvMessage2);

            // delete queue
            await this.sb.DeleteQueueAsync(this.queueName);
        }

        /// <summary>
        /// Creates a test queue and returns it
        /// </summary>
        /// <returns>test queue</returns>
        private async Task<Queue> CreateQueue()
        {
            // create a test queue
            await this.sb.CreateQueueAsync("TestQueue");
            var sbq = await Server.Messaging.ServiceBusQueue.Create(ConfigurationManager.AppSettings["ServiceBusConnectionString"], this.queueName, 0);
            Queue testQueue = new Queue(sbq);
            return testQueue;
        }

        /// <summary>
        /// Create and send a queue message
        /// </summary>
        /// <param name="queue">queue to send it to</param>
        /// <returns>queue message that was sent</returns>
        private async Task<TestQueueMessage> SendMessage(Queue queue)
        {
            // values to be used inside a queue message
            string testMessageString = "sharad";
            int testMessageInt = new Random().Next();
            bool testMessageBool = true;

            // create a test message
            var m = new TestQueueMessage();
            m.TestString = testMessageString;
            m.TestInt = testMessageInt;
            m.TestBool = testMessageBool;

            // send the test message
            await queue.SendAsync(m);

            // return the message
            return m;
        }

        /// <summary>
        /// Simple queue message definition
        /// </summary>
        public class TestQueueMessage : QueueMessage
        {
            /// <summary>
            /// Gets or sets a string
            /// </summary>
            public string TestString { get; set; }

            /// <summary>
            /// Gets or sets an integer
            /// </summary>
            public int TestInt { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether a boolean is true
            /// </summary>
            public bool TestBool { get; set; }

            /// <summary>
            /// Is this object equal?
            /// </summary>
            /// <param name="obj">object to compare to</param>
            /// <returns>true if equal, false otherwise</returns>
            public override bool Equals(object obj)
            {
                // If parameter is null return false.
                if (obj == null)
                {
                    return false;
                }

                // If parameter cannot be cast to TestQueueMessage return false.
                TestQueueMessage t = obj as TestQueueMessage;
                if ((object)t == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (this.TestString == t.TestString) && (this.TestInt == t.TestInt) && (this.TestBool == t.TestBool);
            }

            /// <summary>
            /// Is this object equal?
            /// </summary>
            /// <param name="t">object to compare to</param>
            /// <returns>true if equal, false otherwise</returns>
            public bool Equals(TestQueueMessage t)
            {
                // If parameter is null return false:
                if ((object)t == null)
                {
                    return false;
                }

                // Return true if the fields match:
                return (this.TestString == t.TestString) && (this.TestInt == t.TestInt) && (this.TestBool == t.TestBool);
            }

            /// <summary>
            /// Gets hash code for this object
            /// </summary>
            /// <returns>integer hash code</returns>
            public override int GetHashCode()
            {
                return this.TestString.GetHashCode() ^ this.TestInt.GetHashCode() ^ this.TestBool.GetHashCode();
            }
        }
    }
}
