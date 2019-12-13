// <copyright file="ReportsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    using SocialPlus.Models;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// Reports queue class.
    /// This will send a queue message to a worker role indicating
    /// that a report needs to be submitted for inspection.
    /// </summary>
    public class ReportsQueue : QueueBase, IReportsQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsQueue"/> class
        /// </summary>
        /// <param name="queueManager">Queue manager</param>
        public ReportsQueue(IQueueManager queueManager)
            : base(queueManager)
        {
            this.QueueIdentifier = QueueIdentifier.Reports;
        }

        /// <summary>
        /// Send report message againt a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        public async Task SendUserReportMessage(string appHandle, string reportHandle, Uri callbackUri)
        {
            ReportMessage message = new ReportMessage()
            {
                ReportType = ReportType.User,
                AppHandle = appHandle,
                ReportHandle = reportHandle,
                CallbackUri = callbackUri
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }

        /// <summary>
        /// Send report message againt content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used  to post the result of a review</param>
        /// <returns>Send message task</returns>
        public async Task SendContentReportMessage(string appHandle, string reportHandle, Uri callbackUri)
        {
            ReportMessage message = new ReportMessage()
            {
                ReportType = ReportType.Content,
                AppHandle = appHandle,
                ReportHandle = reportHandle,
                CallbackUri = callbackUri
            };

            Queue queue = await this.QueueManager.GetQueue(this.QueueIdentifier);
            await queue.SendAsync(message);
        }
    }
}
