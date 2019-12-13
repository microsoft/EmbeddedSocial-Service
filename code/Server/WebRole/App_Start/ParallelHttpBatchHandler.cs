// <copyright file="ParallelHttpBatchHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.App_Start
{
    using System.Web.Http;
    using System.Web.Http.Batch;

    /// <summary>
    /// Parallel HTTP batch handler class
    /// </summary>
    public class ParallelHttpBatchHandler : DefaultHttpBatchHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelHttpBatchHandler"/> class.
        /// In the constructor, we just change the execution order from sequential to non-sequential.
        /// </summary>
        /// <param name="httpServer">http server object</param>
        public ParallelHttpBatchHandler(HttpServer httpServer)
            : base(httpServer)
        {
            this.ExecutionOrder = BatchExecutionOrder.Sequential;
        }
    }
}