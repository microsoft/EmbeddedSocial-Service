// <copyright file="IReportsQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Queues
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Reports queue interface
    /// </summary>
    public interface IReportsQueue : IQueueBase
    {
        /// <summary>
        /// Send report message againt a user
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        Task SendUserReportMessage(string appHandle, string reportHandle, Uri callbackUri);

        /// <summary>
        /// Send report message againt content
        /// </summary>
        /// <param name="appHandle">App handle</param>
        /// <param name="reportHandle">Report handle</param>
        /// <param name="callbackUri">URI that can be used to post the result of a review</param>
        /// <returns>Send message task</returns>
        Task SendContentReportMessage(string appHandle, string reportHandle, Uri callbackUri);
    }
}
