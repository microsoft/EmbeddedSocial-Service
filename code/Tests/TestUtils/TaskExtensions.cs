// <copyright file="TaskExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Extension methods for Task
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Method to force a Task to complete within a specified time period
        /// </summary>
        /// <param name="task">Task instance</param>
        /// <param name="millisecondsTimeout">Timeout in Milliseconds</param>
        /// <returns>Completed Task or OperationCanceledException because of timeout</returns>
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
            {
                await task;
            }
            else
            {
                throw new OperationCanceledException();
            }
        }
    }
}
