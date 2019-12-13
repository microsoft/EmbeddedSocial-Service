// <copyright file="QueueWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Queue worker class
    /// </summary>
    public abstract class QueueWorker : IWorker
    {
        /// <summary>
        /// Boolean indicating whether the run loop should continue
        /// </summary>
        private bool keepRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueWorker"/> class.
        /// </summary>
        /// <param name="log">log</param>
        public QueueWorker(ILog log)
        {
            this.Log = log;
        }

        /// <summary>
        /// Gets or sets pointer to the queue
        /// </summary>
        protected IQueueBase Queue { get; set; }

        /// <summary>
        /// Gets or sets log
        /// </summary>
        protected ILog Log { get; set; }

        /// <summary>
        /// Main run loop
        /// </summary>
        public async void Run()
        {
            this.keepRunning = true;
            while (this.keepRunning)
            {
                IMessage message = null;
                bool shouldAbandonMessage = false;

                try
                {
                    message = await this.Queue.ReceiveMessage();
                    if (message != null)
                    {
                        try
                        {
                            await this.Process(message);
                            await this.Queue.CompleteMessage(message);
                        }
                        catch (Exception e)
                        {
                            this.Log.LogError(e);
                            shouldAbandonMessage = true;
                        }

                        if (shouldAbandonMessage)
                        {
                            await this.Queue.AbandonMessage(message);
                        }
                    }
                }
                catch (Exception e)
                {
                    this.Log.LogError(e);
                }
            }
        }

        /// <summary>
        /// Stop method
        /// </summary>
        public void Stop()
        {
            this.keepRunning = false;
        }

        /// <summary>
        /// Process method
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process task</returns>
        protected abstract Task Process(IMessage message);
    }
}
