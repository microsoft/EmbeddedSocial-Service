// <copyright file="ExceptionHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Utils
{
    using System;
    using System.Text;

    /// <summary>
    /// Helper class for exceptions
    /// </summary>
    public class ExceptionHelper
    {
        /// <summary>
        /// Flattens an Exception to a string, attempt to format in the same manner as Exception.ToString().
        ///
        /// This code handles two different cases:
        /// 1- standard exceptions, which may have a chain of inner exceptions.  Inner exceptions are used when exceptions are caught and then rethrown.
        /// 2- aggregate exceptions, which are generated when tasks and aysnc/await are used.  Aggregate exceptions contain a list of exceptions which
        ///    may have occured in parallel on different tasks.
        /// </summary>
        /// <param name="e">exception</param>
        /// <returns>formatted string with exception details</returns>
        public static string FlattenException(Exception e)
        {
            if (e == null)
            {
                return null;
            }

            // if the exception is an aggregate, use FormatAggregate to handle it
            if (e is AggregateException)
            {
                var ae = (AggregateException)e;
                return FormatAggregateException(ae);
            }
            else
            {
                // use FormatException to handle normal exceptions
                return FormatException(e);
            }
        }

        /// <summary>
        /// Formats a standard exception into a string.  Formatting attempts to follow that of Exception.ToString()
        /// </summary>
        /// <param name="e">exception</param>
        /// <returns>formatted string with exception</returns>
        /// <remarks>assumes that caller checks that e is non-null</remarks>
        private static string FormatException(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            // format the exception type and message into string builder
            sb.Append(e.GetType().ToString());
            sb.Append(": ");
            if (e.Message != null)
            {
                sb.Append(e.Message);
            }

            // handle chain of inner exceptions
            if (e.InnerException != null)
            {
                sb.Append(" ---> ");

                // an inner exception could be an aggregate exception or a standard one.
                // so, we handle both cases by recursively calling FlattenException
                string innerMessage = FlattenException(e.InnerException);
                sb.Append(innerMessage);
                sb.AppendLine("   --- End of inner exception stack trace ---");
            }
            else
            {
                sb.AppendLine();
            }

            // format the stack trace after the chain of inner exceptions
            if (e.StackTrace != null)
            {
                sb.AppendLine(e.StackTrace);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats an aggregate exception into a string.  Because Exception.ToString() does not
        /// provide full details for an aggregate exception, we cannot follow its formatting.
        /// </summary>
        /// <param name="ae">aggregate exception</param>
        /// <returns>formatted string with the aggregate exception</returns>
        /// <remarks>assumes that caller checks that e is non-null</remarks>
        private static string FormatAggregateException(AggregateException ae)
        {
            StringBuilder sb = new StringBuilder();

            // format the type and message into string builder
            sb.Append(ae.GetType().ToString());
            sb.Append(": ");
            if (ae.Message != null)
            {
                sb.Append(ae.Message);
            }

            var innerList = ae.Flatten().InnerExceptions;

            if (innerList.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("   --- Start of aggregate InnerExceptions");
            }

            foreach (var inner in innerList)
            {
                // each entry in the list of InnerExceptions could be an aggregate exception or a standard one.
                // so, we handle both cases by recursively calling FlattenException
                var message = FlattenException(inner);
                sb.Append(message);
            }

            if (innerList.Count > 0)
            {
                sb.AppendLine("   --- End of aggregate InnerExceptions");
            }

            // format the stack trace after the list of inner exceptions
            if (ae.StackTrace != null)
            {
                sb.AppendLine(ae.StackTrace);
            }

            return sb.ToString();
        }
    }
}
