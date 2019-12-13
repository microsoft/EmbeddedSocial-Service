// <copyright file="IWorker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Workers
{
    /// <summary>
    /// Worker interface
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Run interface
        /// </summary>
        void Run();

        /// <summary>
        /// Stop interface
        /// </summary>
        void Stop();
    }
}
