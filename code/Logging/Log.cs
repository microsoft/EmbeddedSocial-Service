// <copyright file="Log.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Microsoft.Extensions.Logging;
    using SocialPlus.Utils;

    /// <summary>
    /// Enum used to select the logging destination
    /// </summary>
    public enum LogDestination
    {
        /// <summary>
        /// send log messages to Visual Studio debug window
        /// </summary>
        Debug,

        /// <summary>
        /// send log messages to a trace source
        /// </summary>
        TraceSource,

        /// <summary>
        /// send log messages to ETW via an event source
        /// </summary>
        EventSource,

        /// <summary>
        /// send log messages to console
        /// </summary>
        Console
    }

    /// <summary>
    /// Provides exception, error, and information logging
    /// </summary>
    public class Log : ILog
    {
        /// <summary>
        /// Default name for category of log messages
        /// </summary>
        public static readonly string DefaultCategoryName = "SocialPlus.Log";

        private ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log"/> class.
        /// </summary>
        /// <param name="logDest">logging destination</param>
        /// <param name="categoryName">category name for log messages</param>
        /// <param name="listener">optional trace listener for the TraceSource logging destination</param>
        public Log(LogDestination logDest, string categoryName, TraceListener listener = null)
        {
            var loggerFactory = new LoggerFactory();
            this.logger = loggerFactory.CreateLogger(categoryName);

            if (logDest == LogDestination.Debug)
            {
                // the following produces debug output visible in the visual studio output window
                loggerFactory.AddDebug();
            }
            else if (logDest == LogDestination.Console)
            {
                // the following produces output visible in a console window
                loggerFactory.AddConsole();
            }
            else if (logDest == LogDestination.TraceSource)
            {
                if (listener == null)
                {
                    throw new ArgumentNullException("listener is null");
                }

                var source = new SourceSwitch("sourceSwitch");
                source.Level = SourceLevels.All;
                loggerFactory.AddTraceSource(source, listener);
            }
            else if (logDest == LogDestination.EventSource)
            {
                // create an event listener
                var eventListener = new LogEventListener();

                // the following produces event source debug output
                loggerFactory.AddEventSourceLogger();
            }
        }

        /// <summary>
        /// logs an error message and throws an exception
        /// </summary>
        /// <param name="errorMessage">string for error message</param>
        /// <param name="e">(optional) exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogException(
            string errorMessage,
            Exception e = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            // We should only show the stack trace if exception is null.
            // Otherwise, GenerateErrorLogMessage will flatten the exception which gives us the stack trace
            bool showStackTrace = false;
            if (e == null)
            {
                showStackTrace = true;
            }

            var logMessage = this.ConstructLogMessage(errorMessage, e, showStackTrace, memberName, sourceFilePath, sourceLineNumber);
            this.logger.LogError(logMessage);

            if (e == null)
            {
                throw new Exception(errorMessage);
            }

            throw e;
        }

        /// <summary>
        /// logs an exception and re-throws the exception
        /// </summary>
        /// <param name="e">exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogException(
            Exception e,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = this.ConstructLogMessage(string.Empty, e, false, memberName, sourceFilePath, sourceLineNumber);
            this.logger.LogError(logMessage);
            throw e;
        }

        /// <summary>
        /// logs an error message (without throwing an exception)
        /// </summary>
        /// <param name="errorMessage">string for error message</param>
        /// <param name="showStackTrace">include a stack trace with the output</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogError(
            string errorMessage,
            bool showStackTrace = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = this.ConstructLogMessage(errorMessage, null, showStackTrace, memberName, sourceFilePath, sourceLineNumber);
            this.logger.LogError(logMessage);
        }

        /// <summary>
        /// logs an error message from an exception (without throwing an exception)
        /// </summary>
        /// <param name="e">exception that was thrown</param>
        /// <param name="showStackTrace">include a stack trace with the output</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogError(
            Exception e,
            bool showStackTrace = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var logMessage = this.ConstructLogMessage(string.Empty, e, showStackTrace, memberName, sourceFilePath, sourceLineNumber);
            this.logger.LogError(logMessage);
        }

        /// <summary>
        /// logs an error message and an exception (without throwing an exception)
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <param name="e">exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogError(
            string errorMessage,
            Exception e,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            // We should only show the stack trace if exception is null.
            // Otherwise, GenerateErrorLogMessage will flatten the exception which gives us the stack trace
            bool showStackTrace = false;
            if (e == null)
            {
                showStackTrace = true;
            }

            var logMessage = this.ConstructLogMessage(errorMessage, e, showStackTrace, memberName, sourceFilePath, sourceLineNumber);
            this.logger.LogError(logMessage);
        }

        /// <summary>
        /// logs an informational message
        /// </summary>
        /// <param name="message">string for message</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        public void LogInformation(
             string message,
             [CallerMemberName] string memberName = "",
             [CallerFilePath] string sourceFilePath = "",
             [CallerLineNumber] int sourceLineNumber = 0)
        {
            message = memberName + ":" + sourceFilePath + ":" + sourceLineNumber + ":" + message;
            this.logger.LogInformation(message);
        }

        /// <summary>
        /// Utility routine used by other methods in this class to construct a log message
        /// </summary>
        /// <param name="errorMessage">string for message</param>
        /// <param name="e">exception that was thrown</param>
        /// <param name="showStackTrace">include a stack trace with the output</param>
        /// <param name="memberName">name of calling member</param>
        /// <param name="sourceFilePath">path to source file</param>
        /// <param name="sourceLineNumber">line number in source file</param>
        /// <returns>a log message</returns>
        private string ConstructLogMessage(
            string errorMessage,
            Exception e,
            bool showStackTrace,
            string memberName,
            string sourceFilePath,
            int sourceLineNumber)
        {
            var logMessage = memberName + ":" + sourceFilePath + ":" + sourceLineNumber + ":" + errorMessage;
            if (e != null)
            {
                logMessage += Environment.NewLine;
                logMessage += ExceptionHelper.FlattenException(e);
            }

            if (showStackTrace)
            {
                StackTrace stack = new StackTrace();
                logMessage += Environment.NewLine + stack;
            }

            return logMessage;
        }
    }
}
