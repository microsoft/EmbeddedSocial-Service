// <copyright file="PerformanceLoggingMessageHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.App_Start
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    using SocialPlus.Logging;
    using SocialPlus.Server.Metrics;

    /// <summary>
    /// Message handler that logs the end to end latency of each call to a controller.
    /// We record the latency using a stopwatch and log the information using an ETW event source.
    /// </summary>
    public class PerformanceLoggingMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Application metrics logger
        /// </summary>
        private readonly IPerformanceMetrics performanceMetrics;

        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceLoggingMessageHandler"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="performanceMetrics">Logger of performance metrics</param>
        public PerformanceLoggingMessageHandler(ILog log, IPerformanceMetrics performanceMetrics)
        {
            this.log = log;
            this.performanceMetrics = performanceMetrics;
        }

        /// <summary>
        /// Process the http request on the inbound and outbound path
        /// </summary>
        /// <param name="request">incoming http request</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>an http response message</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;

            // create a guid and store it in the http context
            Guid requestGuid = Guid.NewGuid();
            HttpContext.Current.Items["RequestGuid"] = requestGuid;

            // log the authorization header
            string auth = request?.Headers?.Authorization?.ToString();

            // The Auth header often contains tokens which we do not want to log.  Rather than run an expensive Regex expression
            // to identify the token correctly, we just trim the string wherever we find 'TK='. This should be robust
            // because our documentation specifies to log the token last.  However, if the token is not found and the auth header
            // does not begin with anon, then we assume the auth header is specified incorrectly and therefore we avoid logging the
            // entire header, just to be conservative.
            if (!string.IsNullOrEmpty(auth))
            {
                // Set index to start trimming from as the end of the string (this should always be > or = 0).
                int indexToStartTrimmingFrom = auth.Length - 1;

                // Look for 'TK=' or uncapitalized variants. If found and smaller than the current index of where to start trimming,
                // set the trimming to start here
                bool foundToken = false;
                int indexOfToken = auth.IndexOf("tk=", StringComparison.CurrentCultureIgnoreCase);
                if (indexOfToken >= 0 && indexOfToken < indexToStartTrimmingFrom)
                {
                    foundToken = true;
                    indexToStartTrimmingFrom = indexOfToken;
                }

                // If 'TK=' is not found, and the auth header does not start with ANON, then to
                // be conservative we simply don't log anything from the auth header
                if (!foundToken && !auth.StartsWith("ANON", StringComparison.CurrentCultureIgnoreCase))
                {
                    indexToStartTrimmingFrom = 0;
                }

                // trim the auth header. Note that Remove can throw ArgumentOutOfRangeException if startIndex is less than zero.
                // -or- startIndex specifies a position that is not within this string.
                auth = auth.Remove(indexToStartTrimmingFrom);
            }

            // log the request method, the uri, the guid, and the auth
            string message = $"Method = {request?.Method?.ToString()}, Url = {request?.RequestUri?.ToString()}, Guid = {requestGuid}, Auth = {auth}";
            this.log.LogInformation(message);

            // create a stopwatch, and use it to record the elapsed time
            var sw = Stopwatch.StartNew();
            response = await base.SendAsync(request, cancellationToken);
            sw.Stop();

            // Logs the latency of the API call
            this.performanceMetrics.ApiLatency(request.Method.ToString(), request.RequestUri.ToString(), requestGuid, response.StatusCode, sw.ElapsedMilliseconds);

            // log the request method, the uri, the guid, the http status, and the elapsed time
            // EndToEndLatencyEventSource.Log.ApiLatency(request.Method.ToString(), request.RequestUri.ToString(), requestGuid, response.StatusCode, sw.ElapsedMilliseconds);
            message = $"Method = {request?.Method?.ToString()}, Url = {request?.RequestUri?.ToString()}, Guid = {requestGuid}, Status = {response?.StatusCode}, Response Time = {sw.ElapsedMilliseconds}";
            this.log.LogInformation(message);

            return response;
        }
    }
}