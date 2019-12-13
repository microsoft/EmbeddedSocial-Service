// <copyright file="LogEventListener.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Logging
{
    using System.Collections.Generic;

    using Microsoft.Diagnostics.Tracing;
    using Microsoft.Extensions.Logging.EventSource;

    /// <summary>
    /// LogEventListener is an event listener that enables event logging for the Microsoft-Extensions-Logging events
    /// </summary>
    internal class LogEventListener : EventListener
    {
        private EventSource loggingEventSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEventListener"/> class.
        /// </summary>
        public LogEventListener()
        {
        }

        /// <summary>
        /// Dispose method disables events
        /// </summary>
        public override void Dispose()
        {
            if (this.loggingEventSource != null)
            {
                this.DisableEvents(this.loggingEventSource);
            }

            base.Dispose();
        }

        /// <summary>
        /// Enables event logging for the Microsoft-Extensions-Logging event source
        /// </summary>
        /// <param name="eventSource">event source</param>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (eventSource.Name == "Microsoft-Extensions-Logging")
            {
                // initialize a string, string dictionary of arguments to pass to the EventSource.
                var args = new Dictionary<string, string>();

                // Set the default level (verbosity) to Informational, and only ask for the formatted messages in this case.
                this.EnableEvents(eventSource, EventLevel.Informational, LoggingEventSource.Keywords.FormattedMessage, args);
                this.loggingEventSource = eventSource;
            }
        }
    }
}
