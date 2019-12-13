// <copyright file="LogPerformanceMetrics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Metrics
{
    using System;
    using System.Net;

    using SocialPlus.Logging;

    /// <summary>
    /// Logging for performance metrics
    /// </summary>
    public class LogPerformanceMetrics : IPerformanceMetrics
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogPerformanceMetrics"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        public LogPerformanceMetrics(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Logs the end-to-end latency of api calls. The Http method, Uri, Guid, and Http response status are dimensions for this metric
        /// </summary>
        /// <param name="method">request method</param>
        /// <param name="uri">uri</param>
        /// <param name="guid">guid</param>
        /// <param name="status">response status</param>
        /// <param name="ms">elapsed time in ms</param>
        public void ApiLatency(string method, string uri, Guid guid, HttpStatusCode status, long ms)
        {
            string logMessage = $"elapsedMs = {ms}, method = {method}, uri = {uri}, guid = {guid.ToString()}, status = {status.ToString()}";
            this.log.LogInformation(logMessage);
        }
    }
}
