// <copyright file="IResourceMetrics.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Metrics
{
    using System;
    using System.Net;

    /// <summary>
    /// Interface for logging resource related metrics
    /// </summary>
    public interface IResourceMetrics
    {
        /// <summary>
        /// Logs latency metric for a Redis call. This metric also reflects the number of redis calls.
        /// </summary>
        /// <param name="latency">the amount of time in miliseconds it took to complete this call</param>
        /// <param name="operation">the operation of the call</param>
        void RedisCall(int latency, string operation);

        /// <summary>
        /// Logs latency metric for an AzureTableStore call. This metric also reflects the number of table store calls.
        /// </summary>
        /// <param name="latency">the amount of time in miliseconds it took to complete this call</param>
        /// <param name="operation">the operation of the call</param>
        void AzureTableCall(int latency, string operation);
    }
}
