// <copyright file="ILog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Logging
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides exception, error, and information logging
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// logs an error message and throws an exception
        /// </summary>
        /// <param name="errorMessage">string for error message</param>
        /// <param name="e">(optional) exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogException(
            string errorMessage,
            Exception e = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// logs an exception and re-throws the exception
        /// </summary>
        /// <param name="e">exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogException(
            Exception e,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// logs an error message (without throwing an exception)
        /// </summary>
        /// <param name="errorMessage">string for error message</param>
        /// <param name="showStackTrace">include a stack trace with the output</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogError(
            string errorMessage,
            bool showStackTrace = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// logs an error message from an exception (without throwing an exception)
        /// </summary>
        /// <param name="e">exception that was thrown</param>
        /// <param name="showStackTrace">include a stack trace with the output</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogError(
            Exception e,
            bool showStackTrace = true,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// logs an error message and an exception (without throwing an exception)
        /// </summary>
        /// <param name="errorMessage">error message</param>
        /// <param name="e">exception that was thrown</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogError(
            string errorMessage,
            Exception e,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);

        /// <summary>
        /// logs an informational message
        /// </summary>
        /// <param name="message">string for message</param>
        /// <param name="memberName">(optional). Do not fill in memberName. The compiler will fill this in.</param>
        /// <param name="sourceFilePath">(optional). Do not fill in sourceFilePath. The compiler will fill this in.</param>
        /// <param name="sourceLineNumber">(optional). Do not fill in sourceLineNumber. The compiler will fill this in.</param>
        void LogInformation(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0);
    }
}
