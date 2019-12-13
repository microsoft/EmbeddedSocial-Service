// <copyright file="TracingThrottleLogger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.RateLimiting
{
    using SocialPlus.Logging;
    using WebApiThrottle;

    /// <summary>
    /// Implement logging functionality for throttling. Calls Alerts.
    /// </summary>
    public class TracingThrottleLogger : IThrottleLogger
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        ///  Initializes a new instance of the <see cref="TracingThrottleLogger"/> class
        /// </summary>
        /// <param name="log">Log</param>
        public TracingThrottleLogger(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Implement the log method described in the IThrottleLoger interface. Calls our Alerts.Error method.
        /// </summary>
        /// <param name="entry">the log entry</param>
        public void Log(ThrottleLogEntry entry)
        {
            this.log.LogError(string.Format(
                "{0} Request {1} from {2} has been throttled (blocked), quota {3}/{4} exceeded by {5}",
                entry.LogDate,
                entry.RequestId,
                entry.ClientKey,
                entry.RateLimit,
                entry.RateLimitPeriod,
                entry.TotalRequests));
        }
    }
}
