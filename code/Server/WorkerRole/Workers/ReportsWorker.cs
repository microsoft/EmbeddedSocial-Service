// <copyright file="ReportsWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    using System.Threading.Tasks;

    using SocialPlus.Logging;
    using SocialPlus.Models;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Messages;
    using SocialPlus.Server.Queues;

    /// <summary>
    /// Reports worker
    /// </summary>
    public class ReportsWorker : QueueWorker
    {
        /// <summary>
        /// Reports manager
        /// </summary>
        private IReportsManager reportsManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsWorker"/> class
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="reportsQueue">Reports queue</param>
        /// <param name="reportsManager">Reports manager</param>
        public ReportsWorker(ILog log, IReportsQueue reportsQueue, IReportsManager reportsManager)
            : base(log)
        {
            this.Log = log;
            this.Queue = reportsQueue;
            this.reportsManager = reportsManager;
        }

        /// <summary>
        /// Process message
        /// </summary>
        /// <param name="message">Queue message</param>
        /// <returns>Process message task</returns>
        protected override async Task Process(IMessage message)
        {
            if (message is ReportMessage)
            {
                ReportMessage reportMessage = message as ReportMessage;
                ProcessType processType = (reportMessage.DequeueCount == 1) ? ProcessType.Backend : ProcessType.BackendRetry;

                if (reportMessage.ReportType == ReportType.Content)
                {
                    await this.reportsManager.SubmitContentReportForReview(processType, reportMessage.AppHandle, reportMessage.ReportHandle, reportMessage.CallbackUri);
                }
                else if (reportMessage.ReportType == ReportType.User)
                {
                    await this.reportsManager.SubmitUserReportForReview(processType, reportMessage.AppHandle, reportMessage.ReportHandle, reportMessage.CallbackUri);
                }
                else
                {
                    this.Log.LogError("Got bad ReportMessage where ReportType is: " + reportMessage.ReportType, false);
                }
            }
        }
    }
}
