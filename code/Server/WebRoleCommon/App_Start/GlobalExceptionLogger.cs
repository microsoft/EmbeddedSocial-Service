// <copyright file="GlobalExceptionLogger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.WebRoleCommon.App_Start
{
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.ExceptionHandling;

    using SocialPlus.Logging;

    /// <summary>
    /// global exception logger for asp.net
    /// </summary>
    public class GlobalExceptionLogger : IExceptionLogger
    {
        /// <summary>
        /// Log
        /// </summary>
        private readonly ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionLogger"/> class.
        /// </summary>
        /// <param name="log">Log</param>
        public GlobalExceptionLogger(ILog log)
        {
            this.log = log;
        }

        /// <summary>
        /// Log exception using the Alerts library
        /// </summary>
        /// <param name="context">exception context</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>a task</returns>
        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                this.log.LogError(
                    "Exception occured on: " + context.Request.Method + " " + context.Request.RequestUri,
                    context.Exception);
            });
            return t;
        }
    }
}
