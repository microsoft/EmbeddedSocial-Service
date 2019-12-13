// <copyright file="InstrumentedHttpMessageHandler.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.TestUtils
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// This is a simple HTTP message handler, used primarily to test daisychaining of http handlers
    /// </summary>
    public class InstrumentedHttpMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Issues the HTTP Request
        /// </summary>
        /// <param name="request">HTTP request to issue</param>
        /// <param name="cancellationToken">cancellation token for the request</param>
        /// <returns>HTTP response from server</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // log entry
            Console.Write("entering InstrumentedHttpMessageHandler");

            // check request
            if (request == null || request.RequestUri == null || request.Method == null)
            {
                throw new Exception("HttpRequestMessage is broken - at least one key component is null");
            }

            // log some basic information about the request
            Console.Write("; sending a request to: " + request.RequestUri);
            Console.Write("; operation is a: " + request.Method);
            Console.WriteLine();

            // start the timer
            var sw = Stopwatch.StartNew();

            // issue the HTTP request
            var response = await base.SendAsync(request, cancellationToken);

            // stop the timer
            sw.Stop();

            // log the time
            Console.Write("it took: " + sw.ElapsedMilliseconds + "ms");
            Console.Write("; status code: " + response.StatusCode);

            // log exit
            Console.Write("; exiting InstrumentedHttpMessageHandler");
            Console.WriteLine();

            // return the response
            return response;
        }
    }
}