// <copyright file="EndToEndLatencyEventSource.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.EventSources
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Net;

    /// <summary>
    /// Event source for monitoring end to end latency of api calls
    /// </summary>
    [EventSource(Name = "AlecwTest-SocialPlus-EndToEndLatency")]
    public sealed class EndToEndLatencyEventSource : EventSource
    {
        // every event in an event source must have a unique id (assigned at compile time, thus the use of const)
        private const int ApiLatencyEventId = 1;

        private static EndToEndLatencyEventSource log = new EndToEndLatencyEventSource();

        private EndToEndLatencyEventSource()
        {
        }

        /// <summary>
        /// Gets the object instance.
        /// Event sources must be singleton objects.
        /// </summary>
        public static EndToEndLatencyEventSource Log
        {
            get { return log; }
        }

        /// <summary>
        /// Write an event that logs the Http method, the Uri, the Guid we assign, the Http response status, and the latency
        /// </summary>
        /// <param name="method">request method</param>
        /// <param name="uri">uri</param>
        /// <param name="guid">guid</param>
        /// <param name="status">response status</param>
        /// <param name="ms">elapsed time in ms</param>
        [Event(ApiLatencyEventId, Message = "Method={0}, Uri={1}, Guid={2}, ResponseStatus={3}, ResponseTime={4} ms", Level = EventLevel.Informational, Version = 1)]
        public void ApiLatency(string method, string uri, Guid guid, HttpStatusCode status, long ms)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(ApiLatencyEventId, method, uri, guid, status, ms);
            }
        }
    }
}
